using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// Macro files: plain text commands in StreamingAssets/macros/*.macro, which ship next to the built app
    /// so experts can edit them without a rebuild. A file's name is the macro's name, with spaces,
    /// underscores and dashes treated alike ("heart_tour.macro" runs as "heart tour"). Files are read fresh
    /// on every run, so an edit takes effect the next time the macro runs.
    /// </summary>
    public static class MacroLibrary
    {
        public const string Extension = ".macro";

        public static string Folder => Path.Combine(Application.streamingAssetsPath, "macros");

        /// <summary>Canonical lookup key for a macro name: lowercased, separators collapsed to one space.</summary>
        public static string ToKey(string name) => Regex.Replace(name.Trim().ToLowerInvariant(), @"[\s_\-]+", " ");

        public static IEnumerable<string> Names => Files().Select(path => ToKey(Path.GetFileNameWithoutExtension(path))).OrderBy(n => n);

        public static bool Exists(string name) => FindPath(name) != null;

        public static string FindPath(string name)
        {
            string key = ToKey(name);
            if (key.Length == 0)
                return null;
            return Files().FirstOrDefault(path => ToKey(Path.GetFileNameWithoutExtension(path)) == key);
        }

        /// <summary>The slot a template macro fills with the part it's called with.</summary>
        public const string PartSlot = "{part}";

        /// <summary>A macro containing {part} is a template: "explain aorta" runs explain.macro with
        /// {part} = aorta, so experts can add their own verbs without code.</summary>
        public static bool IsTemplate(string name) => TryRead(name, out string text, out _) && text.Contains(PartSlot);

        /// <summary>Splits "explain left ventricle" into a macro name and its argument, preferring the
        /// longest macro name that starts the text (on a word boundary).</summary>
        public static bool TryMatch(string text, out string name, out string argument)
        {
            string key = ToKey(text);
            name = Names.Where(n => key == n || key.StartsWith(n + " ")).OrderByDescending(n => n.Length).FirstOrDefault();
            if (name == null)
            {
                argument = null;
                return false;
            }
            // The argument keeps its original text (quotes, casing); skip as many words as the name has.
            int nameWords = name.Split(' ').Length;
            var words = text.Trim().Split(new[] { ' ', '\t', '_' }, StringSplitOptions.RemoveEmptyEntries);
            argument = string.Join(" ", words.Skip(nameWords)).Trim();
            return true;
        }

        /// <summary>Macro text with {part} filled in.</summary>
        public static string Expand(string text, string part) => string.IsNullOrEmpty(part) ? text : text.Replace(PartSlot, part);

        public static bool TryRead(string name, out string text, out string path)
        {
            text = null;
            path = FindPath(name);
            if (path == null)
                return false;
            try
            {
                text = File.ReadAllText(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MacroLibrary] Could not read {path}: {e.Message}");
                return false;
            }
        }

        private static IEnumerable<string> Files()
        {
            try
            {
                return Directory.Exists(Folder) ? Directory.GetFiles(Folder, "*" + Extension) : new string[0];
            }
            catch (Exception)
            {
                return new string[0];
            }
        }
    }
}
