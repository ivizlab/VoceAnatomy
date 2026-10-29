using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    public enum Side { Any, Left, Right }

    /// <summary>One named object in the model: a part, a group node, or a text label.</summary>
    public class PartEntry
    {
        /// <summary>Display name in the primary language, e.g. "Heart".</summary>
        public string Name;
        public Side Side;
        /// <summary>A text label (the model's ".t" landmark labels) rather than a part. Labels only count
        /// when no part has the same name.</summary>
        public bool IsLabel;
        /// <summary>The scene object; null in EditMode tests.</summary>
        public GameObject Node;
    }

    public enum PartMatchKind { NotFound, Ambiguous, Exact, Alias, Plural, StartsWith, AllWords, Fuzzy }

    public class PartLookup
    {
        public string Query;
        public PartMatchKind Kind;
        public List<PartEntry> Entries = new List<PartEntry>();
        /// <summary>Distinct names that matched equally well, when <see cref="Kind"/> is Ambiguous.</summary>
        public List<string> Candidates = new List<string>();

        public bool Found => Entries.Count > 0;

        /// <summary>The distinct display names matched, e.g. "Kidney" for both kidneys.</summary>
        public string MatchedName => string.Join(", ", Entries.Select(e => e.Name).Distinct());

        /// <summary>Why nothing was selected, for the console; null when found.</summary>
        public string Problem
        {
            get
            {
                if (Kind == PartMatchKind.Ambiguous)
                    return $"\"{Query}\" could be: {string.Join(", ", Candidates.Take(8))}{(Candidates.Count > 8 ? ", ..." : "")}";
                if (!Found)
                    return Candidates.Count > 0
                        ? $"no part named \"{Query}\" -- closest: {string.Join(", ", Candidates.Take(5))}"
                        : $"no part named \"{Query}\"";
                return null;
            }
        }
    }

    /// <summary>
    /// Turns a spoken or typed part name into model objects. Pure lookup over strings -- the scene side
    /// (<see cref="PartResolver"/>) fills it -- so matching rules are testable in EditMode.
    ///
    /// Lookup order, first hit wins: exact name or synonym (primary language before other languages),
    /// alias or named group, singular/plural variant, starts-with, all words present, then fuzzy spelling.
    /// "left"/"right" filter by side ("left kidney"), but only once the full phrase failed, since
    /// "left ventricle" is itself a name. Several equally good different names make the result ambiguous
    /// rather than a guess.
    /// </summary>
    public class PartIndex
    {
        // Lower tier wins on an exact match: 0 = primary-language name, 1 = its synonyms, 2 = other languages.
        public const int PrimaryName = 0, PrimarySynonym = 1, OtherLanguage = 2;

        private const int MaxAliasDepth = 4;

        private readonly Dictionary<string, List<(int tier, PartEntry entry)>> byKey = new Dictionary<string, List<(int, PartEntry)>>();
        private readonly Dictionary<string, List<string>> aliases = new Dictionary<string, List<string>>();

        public int EntryCount { get; private set; }

        /// <summary>Distinct primary display names, for tab completion.</summary>
        public IEnumerable<string> Names => byKey.Values.SelectMany(v => v).Where(v => v.tier == PrimaryName && !v.entry.IsLabel)
            .Select(v => v.entry.Name).Distinct(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> AliasNames => aliases.Keys;

        public void Add(PartEntry entry, string name, int tier)
        {
            string key = Normalize(name);
            if (key.Length == 0)
                return;
            if (!byKey.TryGetValue(key, out var list))
                byKey[key] = list = new List<(int, PartEntry)>();
            if (!list.Any(x => x.entry == entry))
            {
                list.Add((tier, entry));
                EntryCount++;
            }
        }

        /// <summary>An extra spoken name, or a named set of parts: "collarbone" -> [clavicle],
        /// "heart chambers" -> [left ventricle, right ventricle, ...]. Returns false if the alias already
        /// existed.</summary>
        public bool AddAlias(string name, IEnumerable<string> parts)
        {
            string key = Normalize(name);
            if (key.Length == 0 || aliases.ContainsKey(key))
                return false;
            aliases[key] = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            return true;
        }

        public PartLookup Resolve(string query) => Resolve(query, 0);

        private PartLookup Resolve(string query, int aliasDepth)
        {
            string text = Normalize(query);

            // The whole phrase as a name first -- "left ventricle" is a name, not a side.
            var result = Lookup(text, Side.Any, aliasDepth, loose: false);

            // Then left/right/both as a side filter, before any loose matching of the whole phrase could
            // turn "left kidney" into some other similar-looking name.
            if (!result.Found)
            {
                Side side = Side.Any;
                var words = text.Split(' ').ToList();
                if (words.Remove("left")) side = Side.Left;
                else if (words.Remove("right")) side = Side.Right;
                else words.Remove("both");

                if (words.Count > 0 && words.Count < text.Split(' ').Length)
                {
                    var sided = Lookup(string.Join(" ", words), side, aliasDepth, loose: true);
                    if (sided.Found || sided.Kind == PartMatchKind.Ambiguous)
                        result = sided;
                }
            }

            if (!result.Found && result.Kind != PartMatchKind.Ambiguous)
                result = Lookup(text, Side.Any, aliasDepth, loose: true);

            result.Query = query;
            return result;
        }

        private PartLookup Lookup(string text, Side side, int aliasDepth, bool loose)
        {
            var result = new PartLookup { Query = text };
            if (text.Length == 0)
                return result;

            // 1. Exact name or synonym.
            if (TryExact(text, side, result))
                return result;

            // 2. Alias or named group.
            if (aliasDepth < MaxAliasDepth && aliases.TryGetValue(text, out var targets))
            {
                foreach (var target in targets)
                {
                    var sub = Resolve(target, aliasDepth + 1);
                    result.Entries.AddRange(FilterSide(sub.Entries, side));
                }
                result.Entries = result.Entries.Distinct().ToList();
                result.Kind = result.Found ? PartMatchKind.Alias : PartMatchKind.NotFound;
                if (result.Found)
                    return result;
            }

            // 3. Singular/plural: "lung" -> "lungs", "kidneys" -> "kidney".
            foreach (var variant in PluralVariants(text))
            {
                if (TryExact(variant, side, result))
                {
                    result.Kind = PartMatchKind.Plural;
                    return result;
                }
            }

            if (!loose)
                return result;

            // 4. Starts with ("hea" -> heart), then 5. every word present in any order ("ventricle left").
            // An ambiguous result stops here: listing the real candidates beats a fuzzy guess.
            var queryWords = text.Split(' ');
            if (TryCandidates(byKey.Keys.Where(k => k.StartsWith(text)), PartMatchKind.StartsWith, side, result)
                || result.Kind == PartMatchKind.Ambiguous)
                return result;
            // Also for one word: "ventricle" should list the ventricles, not fuzzy-match "ventral".
            if (TryCandidates(byKey.Keys.Where(k => AllWordsPresent(queryWords, k)), PartMatchKind.AllWords, side, result)
                || result.Kind == PartMatchKind.Ambiguous)
                return result;

            // 6. Fuzzy spelling: smallest edit distance within a length-based limit; ties are ambiguous.
            int limit = text.Length <= 4 ? 1 : text.Length <= 8 ? 2 : 3;
            var scored = byKey.Keys
                .Where(k => Math.Abs(k.Length - text.Length) <= limit)
                .Select(k => (key: k, distance: Levenshtein(text, k)))
                .Where(x => x.distance <= limit)
                .ToList();
            if (scored.Count > 0)
            {
                int best = scored.Min(x => x.distance);
                if (TryCandidates(scored.Where(x => x.distance == best).Select(x => x.key), PartMatchKind.Fuzzy, side, result))
                    return result;
                if (result.Kind == PartMatchKind.Ambiguous)
                    return result;
            }

            // Not found: offer the nearest names (looser limit) as suggestions.
            result.Kind = PartMatchKind.NotFound;
            result.Candidates = byKey.Keys
                .Where(k => Math.Abs(k.Length - text.Length) <= limit + 2)
                .Select(k => (key: k, distance: Levenshtein(text, k)))
                .Where(x => x.distance <= limit + 2)
                .OrderBy(x => x.distance)
                .SelectMany(x => PrimaryNames(x.key))
                .Distinct().Take(5).ToList();
            return result;
        }

        private bool TryExact(string key, Side side, PartLookup result)
        {
            if (!byKey.TryGetValue(key, out var matches))
                return false;
            int bestTier = matches.Min(m => m.tier);
            var entries = FilterSide(PreferParts(matches.Where(m => m.tier == bestTier).Select(m => m.entry)), side);
            if (entries.Count == 0)
                return false;
            result.Entries = entries;
            result.Kind = PartMatchKind.Exact;
            return true;
        }

        // Matches from a loose step count only if they all name the same thing; otherwise the result is
        // ambiguous and lists the candidates (shortest first, so "biceps" lists "biceps brachii muscle").
        private bool TryCandidates(IEnumerable<string> keys, PartMatchKind kind, Side side, PartLookup result)
        {
            var matches = keys.SelectMany(k => byKey[k]).ToList();
            if (matches.Count == 0)
                return false;
            int bestTier = matches.Min(m => m.tier);
            var entries = FilterSide(PreferParts(matches.Where(m => m.tier == bestTier).Select(m => m.entry)), side);
            var names = entries.Select(e => e.Name).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n.Length).ThenBy(n => n).ToList();
            if (names.Count == 0)
                return false;
            if (names.Count > 1)
            {
                result.Kind = PartMatchKind.Ambiguous;
                result.Candidates = names;
                result.Entries.Clear();
                return false;
            }
            result.Kind = kind;
            result.Entries = entries;
            return true;
        }

        private IEnumerable<string> PrimaryNames(string key) => byKey[key].Where(m => !m.entry.IsLabel).Select(m => m.entry.Name);

        private static List<PartEntry> PreferParts(IEnumerable<PartEntry> entries)
        {
            var list = entries.Distinct().ToList();
            return list.Any(e => !e.IsLabel) ? list.Where(e => !e.IsLabel).ToList() : list;
        }

        // A side filter only applies to things that have sides: "left liver" is still the liver.
        private static List<PartEntry> FilterSide(IEnumerable<PartEntry> entries, Side side)
        {
            var list = entries.ToList();
            if (side == Side.Any || !list.Any(e => e.Side != Side.Any))
                return list;
            return list.Where(e => e.Side == side || e.Side == Side.Any).ToList();
        }

        private static IEnumerable<string> PluralVariants(string text)
        {
            if (text.EndsWith("ies")) yield return text.Substring(0, text.Length - 3) + "y";
            if (text.EndsWith("es")) yield return text.Substring(0, text.Length - 2);
            if (text.EndsWith("s")) yield return text.Substring(0, text.Length - 1);
            if (text.EndsWith("y")) yield return text.Substring(0, text.Length - 1) + "ies";
            yield return text + "s";
            yield return text + "es";
        }

        private static bool AllWordsPresent(string[] queryWords, string key)
        {
            var keyWords = key.Split(' ');
            return queryWords.All(q => keyWords.Any(k => k.StartsWith(q)));
        }

        /// <summary>Lowercase, accents removed ("crâne" == "crane"), punctuation to spaces, a leading
        /// "the" dropped, spaces collapsed.</summary>
        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            var sb = new StringBuilder(text.Length);
            foreach (char c in text.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }
            string normalized = string.Join(" ", sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return normalized.StartsWith("the ") ? normalized.Substring(4) : normalized;
        }

        private static int Levenshtein(string a, string b) => CommandParser.Levenshtein(a, b);
    }
}
