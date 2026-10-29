using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// Builds the <see cref="PartIndex"/> from the model: every NameAndDescription in the scene (names in
    /// all languages and their synonyms, loaded by NamesManagement from Resources/Translations0.txt), plus
    /// hand-curated StreamingAssets/anatomy/aliases.json and groups.json. Built on first use and rebuilt if
    /// the translations finish loading after that.
    ///
    /// Object-name suffixes decide what each node is: ".g" groups and plain parts are selectable, ".t"
    /// text labels only count when no part has the name, and side comes from the suffix's last letter
    /// (".l", ".r", and muscle pieces like ".e3l"/".or").
    /// </summary>
    public static class PartResolver
    {
        public static string AnatomyFolder => Path.Combine(Application.streamingAssetsPath, "anatomy");

        private static PartIndex index;
        private static int builtWithTranslations = -1;

        // ".l", ".r", ".ol", ".e3r", ".o12l" ... -- the side is the suffix's last letter.
        private static readonly Regex SideSuffix = new Regex(@"\.[a-z]*\d*([lr])$", RegexOptions.IgnoreCase);
        private static readonly Regex IgnoredSuffix = new Regex(@"\.[ij]$", RegexOptions.IgnoreCase);

        /// <summary>True once the scene's parts exist (Play mode, MainScene). Command validation only
        /// checks part names when this is true.</summary>
        public static bool IsReady => Application.isPlaying && GlobalVariables.Instance != null && GlobalVariables.Instance.allNameScripts != null;

        public static PartLookup Resolve(string query)
        {
            EnsureIndex();
            return index.Resolve(query);
        }

        public static IEnumerable<string> Names
        {
            get
            {
                EnsureIndex();
                return index.Names.Concat(index.AliasNames);
            }
        }

        /// <summary>Forces a rebuild on next use, e.g. after editing aliases.json.</summary>
        public static void Invalidate() => index = null;

        private static void EnsureIndex()
        {
            if (!IsReady)
            {
                index = index ?? new PartIndex();
                return;
            }
            int translated = GlobalVariables.Instance.allNameScripts.Count(n => n != null && n.allNames != null && n.allNames.Length > 0);
            if (index != null && translated == builtWithTranslations)
                return;
            builtWithTranslations = translated;
            index = Build();
        }

        private static PartIndex Build()
        {
            var built = new PartIndex();
            int primaryLanguage = Mathf.Max(0, Settings.languageIndex);

            foreach (var nameScript in GlobalVariables.Instance.allNameScripts)
            {
                if (nameScript == null)
                    continue;
                var node = nameScript.gameObject;
                // At runtime the app renames sided objects ("Kidney.l" -> "Kidney (L)"); the original
                // name keeps the suffix that says what the node is.
                string originalName = string.IsNullOrEmpty(nameScript.originalName) ? node.name : nameScript.originalName;
                // ".i"/".j" nodes are skipped everywhere else in the app (RecursiveFindChild,
                // SetActiveRecursively), so naming one would show nothing sensible.
                if (node.CompareTag("Insertions") || node.GetComponent<Line>() != null || IgnoredSuffix.IsMatch(originalName))
                    continue;

                bool isLabel = node.GetComponent<Label>() != null;
                bool isPart = node.GetComponent<TangibleBodyPart>() != null;
                // Group nodes (".g") have no mesh themselves; anything with no parts under it at all is
                // skipped, since naming it could never show anything.
                if (!isLabel && !isPart && node.GetComponentInChildren<TangibleBodyPart>(true) == null)
                    continue;

                string objectName = originalName.RemoveSuffix().Trim();
                var names = nameScript.allNames;
                var synonyms = nameScript.allSynonyms;
                string display = names != null && names.Length > primaryLanguage && !string.IsNullOrWhiteSpace(names[primaryLanguage])
                    ? names[primaryLanguage].Trim()
                    : objectName;

                var match = SideSuffix.Match(originalName);
                var entry = new PartEntry
                {
                    Name = display,
                    IsLabel = isLabel,
                    Node = node,
                    Side = !match.Success ? Side.Any : char.ToLowerInvariant(match.Groups[1].Value[0]) == 'l' ? Side.Left : Side.Right,
                };

                built.Add(entry, display, PartIndex.PrimaryName);
                built.Add(entry, objectName, PartIndex.PrimaryName);
                if (names != null)
                {
                    for (int language = 0; language < names.Length; language++)
                    {
                        // English always counts as primary too: commands and macros are written in English.
                        bool primary = language == primaryLanguage || language == 0;
                        built.Add(entry, names[language], primary ? PartIndex.PrimaryName : PartIndex.OtherLanguage);
                        if (synonyms != null && language < synonyms.Count && synonyms[language] != null)
                        {
                            foreach (var synonym in synonyms[language])
                                built.Add(entry, synonym, primary ? PartIndex.PrimarySynonym : PartIndex.OtherLanguage);
                        }
                    }
                }
            }

            LoadAliases(built, "aliases.json");
            LoadAliases(built, "groups.json");
            CommandLog.Info($"part index: {built.EntryCount} names for {GlobalVariables.Instance.allNameScripts.Count} objects");
            return built;
        }

        [Serializable]
        private class AliasFile
        {
            public AliasEntry[] aliases = new AliasEntry[0];
            public AliasEntry[] groups = new AliasEntry[0];
        }

        [Serializable]
        private class AliasEntry
        {
            public string name;
            public string[] parts = new string[0];
        }

        // aliases.json and groups.json share one shape -- a name and the parts it stands for -- and both
        // are checked here, so a typo in either is reported at startup rather than mid-lecture.
        private static void LoadAliases(PartIndex target, string fileName)
        {
            string path = Path.Combine(AnatomyFolder, fileName);
            if (!File.Exists(path))
                return;

            AliasFile file;
            try
            {
                file = JsonUtility.FromJson<AliasFile>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                CommandLog.Error($"{fileName}: not valid JSON ({e.Message})");
                return;
            }
            if (file == null)
                return;

            foreach (var entry in (file.aliases ?? new AliasEntry[0]).Concat(file.groups ?? new AliasEntry[0]))
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.name) || entry.parts == null || entry.parts.Length == 0)
                    continue;
                if (!target.AddAlias(entry.name, entry.parts))
                {
                    CommandLog.Warning($"{fileName}: \"{entry.name}\" is defined twice -- keeping the first");
                    continue;
                }
                foreach (var part in entry.parts)
                {
                    var lookup = target.Resolve(part);
                    if (!lookup.Found)
                        CommandLog.Warning($"{fileName}: \"{entry.name}\" -> {lookup.Problem}");
                }
            }
        }
    }
}
