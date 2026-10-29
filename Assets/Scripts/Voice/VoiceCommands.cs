using Assets.Scripts.Scripting;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Push-to-talk voice commands for the presenter demo. Hold <see cref="triggerKey"/>
    /// (or the mouse middle button -- a second, independent trigger, e.g. for a wireless
    /// mouse kept near the Leap controller) to listen, release to stop -- recognition
    /// audio is only captured while a trigger is held, so ambient room conversation is
    /// never processed.
    ///
    /// This class is now just the four pipeline stages wired together, each independently
    /// testable/swappable:
    /// capture (push-to-talk timing + <see cref="VoiceProcessor"/>, unchanged) -> transcribe
    /// (<see cref="IAsrBackend"/>, the Whisper sidecar configured by <see cref="AsrConfig"/>) -> interpret
    /// (<see cref="VoiceInterpreter"/>: normalize + match into command text, no scene dependency) ->
    /// execute (<see cref="CommandRunner"/>, the same runner typed commands and macros use).
    /// </summary>
    [RequireComponent(typeof(VoiceProcessor))]
    public class VoiceCommands : MonoBehaviour
    {
        [Header("Push-to-talk trigger")]
        public Key triggerKey = Key.V;

        [Tooltip("Keeps the mic capturing for this long after the key is released, so the tail of the last word isn't cut off before the ASR backend finalizes it. Increase if the last word of a phrase keeps getting dropped; decrease if commands feel laggy.")]
        public float trailingCaptureSeconds = 0.6f;

        [Header("UI")]
        public VoiceListeningIndicator listeningIndicator;

        private VoiceProcessor voiceProcessor;
        private AsrConfig config;
        private IAsrBackend asrBackend;

        private bool voiceReady;
        private bool listening;
        private bool capturing;
        private Coroutine stoppingCoroutine;
        private int captureSampleRate = 16000;

        private void Awake()
        {
            voiceProcessor = GetComponent<VoiceProcessor>();
            config = AsrConfig.Load(Path.Combine(Application.streamingAssetsPath, "asr_config.json"));
            StartCoroutine(InitAsr());
        }

        private IEnumerator InitAsr()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[VoiceCommands] No microphone detected -- voice input disabled. App will run normally otherwise.");
                yield break;
            }

            ResolveMicrophoneDevice();

            // Some mics (webcams especially) don't actually support 16kHz -- Unity silently
            // clamps to whatever the device supports, so backends that need 16kHz (Whisper)
            // resample from whatever we actually captured at.
            Microphone.GetDeviceCaps(voiceProcessor.CurrentDeviceName, out int minFreq, out int maxFreq);
            captureSampleRate = (minFreq == 0 && maxFreq == 0) ? 16000 : Mathf.Clamp(16000, minFreq, maxFreq);
            Debug.Log($"[VoiceCommands] Capturing microphone \"{voiceProcessor.CurrentDeviceName}\" at {captureSampleRate} Hz.");

            // Export the vocabulary this matcher just built, so the Whisper sidecar's
            // initial_prompt is generated from the same single source of truth as the matcher
            // rather than a hand-maintained duplicate.
            VoiceInterpreter.Matcher.ExportVocabulary(Path.Combine(Application.streamingAssetsPath, "asr_vocabulary.json"));

            var whisper = new WhisperAsrBackend(config.WhisperBaseUrl, config.WhisperTimeoutSeconds,
                config.AutoStartSidecar ? config.SidecarDirectory : null, config.SidecarStartupTimeoutSeconds);
            // Assigned before Initialize() so OnDestroy still shuts down a sidecar this
            // backend launched if play mode ends while it's still warming up.
            asrBackend = whisper;
            yield return whisper.Initialize();

            if (!asrBackend.IsReady)
            {
                Debug.LogWarning("[VoiceCommands] Whisper ASR backend failed to become ready -- voice input disabled. App will run normally otherwise.");
                yield break;
            }

            voiceProcessor.OnFrameCaptured += OnFrameCaptured;
            voiceReady = true;
            Debug.Log("[VoiceCommands] Whisper backend ready -- listening for push-to-talk.");
        }

        // Selects the mic by name substring rather than Unity's serialized device index --
        // USB device indices reorder between reboots/unplug cycles, which would otherwise
        // silently break a demo. Empty configured name keeps whatever device index
        // VoiceProcessor already defaults to.
        private void ResolveMicrophoneDevice()
        {
            voiceProcessor.UpdateDevices();

            if (!string.IsNullOrWhiteSpace(config.MicrophoneNameContains))
            {
                int index = voiceProcessor.Devices.FindIndex(
                    d => d.IndexOf(config.MicrophoneNameContains, StringComparison.OrdinalIgnoreCase) >= 0);

                if (index >= 0)
                {
                    if (index != voiceProcessor.CurrentDeviceIndex)
                        voiceProcessor.ChangeDevice(index);
                }
                else
                {
                    Debug.LogWarning($"[VoiceCommands] No microphone device name contains \"{config.MicrophoneNameContains}\" -- using default device.");
                }
            }

            Debug.Log($"[VoiceCommands] Resolved microphone device: \"{voiceProcessor.CurrentDeviceName}\".");
        }

        private void Update()
        {
            if (!voiceReady)
                return;

            // Two independent trigger sources (keyboard key, mouse middle-click) both hold
            // push-to-talk active -- combined as "is either currently held" rather than
            // separate press/release edges, so releasing one while the other is still held
            // (e.g. tapping the mouse button mid-sentence while holding the key) doesn't cut
            // listening off early.
            // The key is ignored while a text field (e.g. the command console) has focus, so
            // typing a "v" doesn't start listening; the mouse trigger still works.
            bool triggerHeld = false;
            if (Keyboard.current != null && !Shortcuts.UserIsWriting())
                triggerHeld |= Keyboard.current[triggerKey].isPressed;
            if (Mouse.current != null)
                triggerHeld |= Mouse.current.middleButton.isPressed;

            if (triggerHeld && !listening)
                StartListening();
            else if (!triggerHeld && listening)
                StopListening();
        }

        private void StartListening()
        {
            if (listening)
                return;

            // A key-up/key-down cycle faster than the trailing capture window below
            // (e.g. a quick re-press) should discard the previous utterance rather
            // than finalize it late against a freshly-reset backend.
            if (stoppingCoroutine != null)
            {
                StopCoroutine(stoppingCoroutine);
                stoppingCoroutine = null;
            }

            listening = true;
            capturing = true;
            asrBackend.ResetUtterance();
            voiceProcessor.StartRecording(captureSampleRate, 512, false);
            listeningIndicator?.Show();
            Debug.Log("[VoiceCommands] Listening...");
        }

        private void StopListening()
        {
            if (!listening)
                return;
            listening = false;
            listeningIndicator?.Hide();
            stoppingCoroutine = StartCoroutine(FinishListening());
        }

        private IEnumerator FinishListening()
        {
            // Keep the mic capturing briefly after key-up before stopping it and
            // finalizing -- releasing the key right as the last word ends otherwise
            // truncates it, since any audio not yet delivered to the backend at the moment
            // StopRecording() runs is lost.
            yield return new WaitForSecondsRealtime(trailingCaptureSeconds);

            capturing = false;
            voiceProcessor.StopRecording();

            AsrResult asrResult = AsrResult.Fail("no result");
            yield return asrBackend.FinalizeUtterance(result => asrResult = result);

            if (!asrResult.Success)
            {
                // Never hang, never block input -- log it, give a brief non-blocking visual
                // cue, and return to idle ready state. A missed command is recoverable in a
                // live demo; a frozen app is not.
                Debug.LogWarning($"[VoiceCommands] ASR call failed or timed out: {asrResult.ErrorMessage}");
                StartCoroutine(FlashIndicatorBriefly());
                stoppingCoroutine = null;
                yield break;
            }

            // The raw transcript is the durable record -- log it unaltered, including its
            // original punctuation and casing, before anything derives from it.
            // The interpreter logs raw -> normalized -> command itself, to both the Unity log
            // and the command console.
            string commandText = VoiceInterpreter.Interpret(asrResult.Text ?? "");
            if (commandText != null)
            {
                if (CommandRunner.Instance != null)
                    CommandRunner.Instance.Submit(commandText, CommandSource.Voice);
                else
                    Debug.LogWarning("[VoiceCommands] No CommandRunner in the scene -- voice command not run.");
            }

            stoppingCoroutine = null;
        }

        private IEnumerator FlashIndicatorBriefly()
        {
            listeningIndicator?.Show();
            yield return new WaitForSeconds(0.75f);
            listeningIndicator?.Hide();
        }

        private void OnFrameCaptured(short[] samples)
        {
            if (!capturing || asrBackend == null)
                return;
            asrBackend.AcceptFrame(samples, captureSampleRate);
        }

        private void OnDestroy()
        {
            if (voiceProcessor != null)
                voiceProcessor.OnFrameCaptured -= OnFrameCaptured;
            asrBackend?.Shutdown();
        }
    }
}
