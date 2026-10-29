using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Talks to the local faster-whisper sidecar over HTTP -- no PyTorch/CTranslate2 in the
    /// Unity process, just POST audio, get back text. Buffers whole-utterance audio (Whisper
    /// returns one result per utterance, not a growing partial) and resamples to 16kHz
    /// mono once at finalize time rather than per-frame, to avoid resampling boundary artifacts.
    /// Every call has a timeout; failure/timeout never throws and never hangs -- it calls back
    /// with <see cref="AsrResult.Fail"/> so a missed command stays recoverable instead of
    /// freezing the app.
    /// </summary>
    public class WhisperAsrBackend : IAsrBackend
    {
        private const int TargetSampleRate = 16000;
        private static readonly bool IsWindows = Path.DirectorySeparatorChar == '\\';

        private readonly string baseUrl;
        private readonly float timeoutSeconds;
        private readonly string sidecarDirectory;
        private readonly float sidecarStartupTimeoutSeconds;
        private readonly List<short> buffer = new List<short>();
        private int capturedSampleRate = TargetSampleRate;

        // Only set when this backend launched the sidecar itself -- a sidecar that was already
        // running (e.g. started by hand for testing) is never killed on Shutdown().
        private Process sidecarProcess;
        private StreamWriter sidecarLog;
        private string sidecarLogPath;
        private string lastHealthError;

        public bool IsReady { get; private set; }

        /// <param name="sidecarDirectory">Folder containing the sidecar's main.py and venv,
        /// relative to the project root or absolute. Null/empty disables auto-start -- the
        /// sidecar then has to be running already.</param>
        public WhisperAsrBackend(string baseUrl, float timeoutSeconds, string sidecarDirectory = null,
            float sidecarStartupTimeoutSeconds = 60f)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            this.timeoutSeconds = timeoutSeconds;
            this.sidecarDirectory = sidecarDirectory;
            this.sidecarStartupTimeoutSeconds = sidecarStartupTimeoutSeconds;
        }

        /// <summary>Health-checks the sidecar, launching it first if it isn't running and
        /// auto-start is configured. The sidecar doesn't answer /health until its own startup
        /// warm-up inference has completed, so a successful reply here means the presenter's
        /// first real command won't eat the cold-load latency penalty.</summary>
        public IEnumerator Initialize()
        {
            yield return CheckHealth(Mathf.CeilToInt(timeoutSeconds));

            if (!IsReady && !string.IsNullOrWhiteSpace(sidecarDirectory) && StartSidecar())
            {
                float deadline = Time.realtimeSinceStartup + sidecarStartupTimeoutSeconds;
                while (!IsReady && Time.realtimeSinceStartup < deadline && !sidecarProcess.HasExited)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    yield return CheckHealth(2);
                }

                if (!IsReady)
                {
                    Debug.LogWarning(sidecarProcess.HasExited
                        ? "[WhisperAsrBackend] Sidecar exited during startup -- see " + sidecarLogPath + "."
                        : "[WhisperAsrBackend] Sidecar didn't become ready within " + sidecarStartupTimeoutSeconds +
                          "s -- see " + sidecarLogPath + ".");
                    StopSidecar();
                }
            }

            if (IsReady)
                Debug.Log("[WhisperAsrBackend] Sidecar reachable and warmed up at " + baseUrl + ".");
            else
                Debug.LogWarning("[WhisperAsrBackend] Sidecar not reachable at " + baseUrl + " (" + lastHealthError +
                    ") -- voice input disabled until it's running. Launch the sidecar and reload to retry.");
        }

        private IEnumerator CheckHealth(int timeout)
        {
            using (var req = UnityWebRequest.Get(baseUrl + "/health"))
            {
                req.timeout = timeout;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                    IsReady = true;
                else
                    lastHealthError = req.error;
            }
        }

        // Launches the sidecar with its own venv's interpreter, hidden, with its output
        // captured to a log file (there's no console window to read it from otherwise).
        // Returns false -- never throws -- if it can't be launched, so a missing venv degrades
        // to the same "voice input disabled" path as a sidecar that simply isn't running.
        private bool StartSidecar()
        {
            string directory = Path.IsPathRooted(sidecarDirectory)
                ? sidecarDirectory
                : Path.GetFullPath(Path.Combine(Application.dataPath, "..", sidecarDirectory));
            // A venv lays its interpreter out differently per OS.
            string python = IsWindows
                ? Path.Combine(directory, "venv", "Scripts", "python.exe")
                : Path.Combine(directory, "venv", "bin", "python");

            if (!File.Exists(python) || !File.Exists(Path.Combine(directory, "main.py")))
            {
                Debug.LogWarning("[WhisperAsrBackend] Can't auto-start the sidecar: no venv/main.py under " + directory +
                    " -- see its README for setup.");
                return false;
            }

            try
            {
                sidecarLogPath = Path.Combine(Application.persistentDataPath, "asr_sidecar.log");
                sidecarLog = new StreamWriter(sidecarLogPath, false) { AutoFlush = true };

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = python,
                        Arguments = "main.py",
                        WorkingDirectory = directory,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    },
                };
                process.OutputDataReceived += OnSidecarOutput;
                process.ErrorDataReceived += OnSidecarOutput;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                sidecarProcess = process;

                Debug.Log("[WhisperAsrBackend] Sidecar wasn't running -- launched it (pid " + process.Id +
                    "), waiting for model load + warm-up. Log: " + sidecarLogPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WhisperAsrBackend] Failed to auto-start the sidecar: " + e.Message);
                StopSidecar();
                return false;
            }
        }

        // Raised on a thread-pool thread, and stdout/stderr can interleave.
        private void OnSidecarOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            StreamWriter log = sidecarLog;
            if (log == null)
                return;

            lock (log)
            {
                try { log.WriteLine(e.Data); }
                catch (ObjectDisposedException) { }
            }
        }

        private void StopSidecar()
        {
            if (sidecarProcess != null)
            {
                try
                {
                    if (!sidecarProcess.HasExited)
                    {
                        if (IsWindows)
                        {
                            // venv\Scripts\python.exe is a launcher that runs the real interpreter
                            // as a child process, so killing just this pid would orphan the server
                            // (and keep the port + GPU memory held) -- /T takes the whole tree.
                            using (var kill = Process.Start(new ProcessStartInfo
                            {
                                FileName = "taskkill",
                                Arguments = "/PID " + sidecarProcess.Id + " /T /F",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }))
                            {
                                kill.WaitForExit(3000);
                            }
                        }
                        else
                        {
                            // venv/bin/python on macOS/Linux is a symlink to the interpreter
                            // itself, not a launcher -- this pid is the server.
                            sidecarProcess.Kill();
                            sidecarProcess.WaitForExit(3000);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[WhisperAsrBackend] Failed to stop the sidecar: " + e.Message);
                }

                sidecarProcess.Dispose();
                sidecarProcess = null;
            }

            if (sidecarLog != null)
            {
                StreamWriter log = sidecarLog;
                sidecarLog = null;
                lock (log)
                    log.Dispose();
            }
        }

        public void ResetUtterance()
        {
            buffer.Clear();
        }

        public void AcceptFrame(short[] pcm16, int sampleRate)
        {
            capturedSampleRate = sampleRate;
            buffer.AddRange(pcm16);
        }

        public IEnumerator FinalizeUtterance(Action<AsrResult> onComplete)
        {
            short[] samples = buffer.ToArray();
            buffer.Clear();

            if (samples.Length == 0)
            {
                onComplete(AsrResult.Ok(""));
                yield break;
            }

            if (capturedSampleRate != TargetSampleRate)
                samples = Resample(samples, capturedSampleRate, TargetSampleRate);

            byte[] wav = BuildWavBytes(samples, TargetSampleRate);

            using (var req = new UnityWebRequest(baseUrl + "/transcribe", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(wav);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "audio/wav");
                req.timeout = Mathf.CeilToInt(timeoutSeconds);

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onComplete(AsrResult.Fail(req.error));
                    yield break;
                }

                string text;
                try
                {
                    JSONObject json = JSONNode.Parse(req.downloadHandler.text).AsObject;
                    text = json.HasKey("text") ? json["text"].Value : "";
                }
                catch (Exception e)
                {
                    onComplete(AsrResult.Fail("Malformed sidecar response: " + e.Message));
                    yield break;
                }

                onComplete(AsrResult.Ok(text));
            }
        }

        public void Shutdown()
        {
            StopSidecar();
        }

        // Linear-interpolation resample -- adequate for speech at these rates and avoids
        // pulling in a DSP dependency for what's a stopgap until the mic is captured at 16kHz
        // natively.
        private static short[] Resample(short[] input, int fromRate, int toRate)
        {
            if (fromRate == toRate || input.Length == 0)
                return input;

            int outputLength = (int)((long)input.Length * toRate / fromRate);
            short[] output = new short[outputLength];
            double step = (double)fromRate / toRate;

            for (int i = 0; i < outputLength; i++)
            {
                double srcPos = i * step;
                int srcIndex = (int)srcPos;
                double frac = srcPos - srcIndex;
                short a = input[Math.Min(srcIndex, input.Length - 1)];
                short b = input[Math.Min(srcIndex + 1, input.Length - 1)];
                output[i] = (short)(a + (b - a) * frac);
            }

            return output;
        }

        // Minimal 16-bit PCM mono WAV container -- just enough for the sidecar to decode via
        // soundfile/wave, no compression, no extra chunks.
        private static byte[] BuildWavBytes(short[] samples, int sampleRate)
        {
            int dataLength = samples.Length * 2;
            int fileLength = 44 + dataLength;

            using (var stream = new MemoryStream(fileLength))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(fileLength - 8);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // PCM fmt chunk size
                writer.Write((short)1); // PCM
                writer.Write((short)1); // mono
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2); // byte rate = sampleRate * blockAlign
                writer.Write((short)2); // block align (16-bit mono)
                writer.Write((short)16); // bits per sample

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataLength);
                foreach (short sample in samples)
                    writer.Write(sample);

                writer.Flush();
                return stream.ToArray();
            }
        }
    }
}
