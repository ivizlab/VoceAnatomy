using System;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    public enum CommandLogLevel { Echo, Info, Trace, Warning, Error }

    /// <summary>
    /// Where every command, from any source, reports what it did. The console subscribes to
    /// <see cref="Line"/>; everything also goes to the Unity log with a [Cmd] prefix.
    /// </summary>
    public static class CommandLog
    {
        public static event Action<string, CommandLogLevel> Line;

        public static void Echo(string text) => Write(text, CommandLogLevel.Echo);
        public static void Info(string text) => Write(text, CommandLogLevel.Info);
        /// <summary>Voice pipeline steps (raw -> normalized -> command), shown so a phrase that didn't
        /// work can be diagnosed.</summary>
        public static void Trace(string text) => Write(text, CommandLogLevel.Trace);
        public static void Warning(string text) => Write(text, CommandLogLevel.Warning);
        public static void Error(string text) => Write(text, CommandLogLevel.Error);

        private static void Write(string text, CommandLogLevel level)
        {
            switch (level)
            {
                case CommandLogLevel.Warning: Debug.LogWarning("[Cmd] " + text); break;
                case CommandLogLevel.Error: Debug.LogWarning("[Cmd] ERROR " + text); break;
                default: Debug.Log("[Cmd] " + text); break;
            }
            Line?.Invoke(text, level);
        }
    }
}
