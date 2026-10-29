using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// "record heart tour" ... "end": captures every command typed or spoken in between, in its canonical
    /// form, and writes it as a macro file. The quickest way to draft a lesson is to perform it once and
    /// then tidy the file (add waits and captions). Never overwrites an existing macro.
    /// </summary>
    public static class MacroRecorder
    {
        // Commands about the session itself, not the lesson.
        private static readonly HashSet<string> NotRecorded = new HashSet<string> { "record", "end", "stop", "help", "macros", "check" };

        private static readonly List<string> lines = new List<string>();

        public static string RecordingName { get; private set; }
        public static bool IsRecording => RecordingName != null;

        public static void Start(string name)
        {
            RecordingName = MacroLibrary.ToKey(name);
            lines.Clear();
        }

        /// <summary>Called by the runner for every live (console/voice) submission while recording.</summary>
        public static void Capture(IEnumerable<CommandInvocation> commands)
        {
            if (!IsRecording)
                return;
            lines.AddRange(commands.Where(c => !NotRecorded.Contains(c.Verb)).Select(c => c.ToString()));
        }

        /// <summary>Writes the recording and stops recording. Returns the file path, or null on failure.</summary>
        public static string Finish(out int commandCount, out string error)
        {
            commandCount = lines.Count;
            error = null;
            string name = RecordingName;
            RecordingName = null;

            try
            {
                Directory.CreateDirectory(MacroLibrary.Folder);
                string baseName = name.Replace(' ', '_');
                string path = Path.Combine(MacroLibrary.Folder, baseName + MacroLibrary.Extension);
                for (int n = 2; File.Exists(path); n++)
                    path = Path.Combine(MacroLibrary.Folder, $"{baseName}_{n}{MacroLibrary.Extension}");

                var text = new List<string>
                {
                    $"# {Path.GetFileName(path)} -- recorded {DateTime.Now:yyyy-MM-dd HH:mm}. Edit freely: add wait N, wait click, captions.",
                };
                text.AddRange(lines);
                File.WriteAllLines(path, text);
                return path;
            }
            catch (Exception e)
            {
                error = e.Message;
                return null;
            }
            finally
            {
                lines.Clear();
            }
        }
    }
}
