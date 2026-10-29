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
