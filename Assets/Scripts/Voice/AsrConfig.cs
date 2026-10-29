using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Loads the ASR settings from a plain JSON config file (not hardcoded, not a Unity
    /// ScriptableObject asset) so the whisper section can be tuned without recompiling. Falls
    /// back to safe defaults if the file is missing or malformed, rather than crashing the app.
    /// </summary>
    public class AsrConfig
    {
        public bool FuzzyMatchingEnabled = true;

        // Substring match against Microphone.devices, not an index -- USB device indices
        // reorder between reboots/unplug cycles, which would otherwise silently break a demo.
        // Empty means "use the first available device", same as before this config existed.
        public string MicrophoneNameContains = "";

        public string WhisperBaseUrl = "http://127.0.0.1:8765";
        public float WhisperTimeoutSeconds = 8f;

        // When the sidecar isn't already answering at WhisperBaseUrl, launch it from
        // SidecarDirectory (relative to the project root, or absolute) and wait up to
        // SidecarStartupTimeoutSeconds for its model load + warm-up. A sidecar that was
        // already running (e.g. launched by hand for testing) is reused and left alone.
        public bool AutoStartSidecar = true;
        public string SidecarDirectory = "python/asr_sidecar";
        public float SidecarStartupTimeoutSeconds = 60f;

        public static AsrConfig Load(string path)
        {
            var config = new AsrConfig();

            if (!File.Exists(path))
            {
                Debug.LogWarning("[AsrConfig] No config file at " + path + " -- using defaults.");
                return config;
            }

            try
            {
                JSONObject root = JSONNode.Parse(File.ReadAllText(path)).AsObject;

                if (root.HasKey("fuzzyMatchingEnabled"))
                    config.FuzzyMatchingEnabled = root["fuzzyMatchingEnabled"].AsBool;
                if (root.HasKey("microphoneNameContains"))
                    config.MicrophoneNameContains = root["microphoneNameContains"].Value;

                if (root.HasKey("whisper"))
                {
                    JSONObject whisper = root["whisper"].AsObject;
                    if (whisper.HasKey("baseUrl"))
                        config.WhisperBaseUrl = whisper["baseUrl"].Value;
                    if (whisper.HasKey("timeoutSeconds"))
                        config.WhisperTimeoutSeconds = whisper["timeoutSeconds"].AsFloat;
                    if (whisper.HasKey("autoStartSidecar"))
                        config.AutoStartSidecar = whisper["autoStartSidecar"].AsBool;
                    if (whisper.HasKey("sidecarDirectory"))
                        config.SidecarDirectory = whisper["sidecarDirectory"].Value;
                    if (whisper.HasKey("sidecarStartupTimeoutSeconds"))
                        config.SidecarStartupTimeoutSeconds = whisper["sidecarStartupTimeoutSeconds"].AsFloat;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AsrConfig] Failed to parse " + path + ": " + e.Message + " -- using defaults.");
                return new AsrConfig();
            }

            return config;
        }
    }
}
