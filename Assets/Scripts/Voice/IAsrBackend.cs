using System;
using System.Collections;

namespace Assets.Scripts.Voice
{
    /// <summary>Outcome of one push-to-talk utterance. Text is the verbatim raw transcript --
    /// callers must log/store it unmodified before doing anything else with it.</summary>
    public struct AsrResult
    {
        public bool Success;
        public string Text;
        public string ErrorMessage;

        public static AsrResult Ok(string text) => new AsrResult { Success = true, Text = text };
        public static AsrResult Fail(string errorMessage) => new AsrResult { Success = false, ErrorMessage = errorMessage };
    }

    /// <summary>
    /// One push-to-talk utterance's worth of audio in, one transcript out. The Whisper
    /// sidecar implements this today; keeping it behind an interface lets
    /// <see cref="VoiceCommands"/> stay engine-agnostic if the ASR engine is swapped later.
    /// </summary>
    public interface IAsrBackend
    {
        /// <summary>False until model load (and, for Whisper, warm-up) has completed.</summary>
        bool IsReady { get; }

        /// <summary>Called once per push-to-talk press, before any frames for the new
        /// utterance arrive.</summary>
        void ResetUtterance();

        /// <summary>Called for every captured audio frame while the trigger is held (and
        /// during the short trailing-capture window after release).</summary>
        void AcceptFrame(short[] pcm16, int sampleRate);

        /// <summary>
        /// Called once after capture stops. Must never throw and must always eventually call
        /// onComplete -- on a sidecar timeout or connection failure, call it with
        /// <see cref="AsrResult.Fail"/> rather than hanging, so a missed command never becomes a
        /// frozen app.
        /// </summary>
        IEnumerator FinalizeUtterance(Action<AsrResult> onComplete);

        void Shutdown();
    }
}
