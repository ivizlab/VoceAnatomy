using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Decides what command a (normalized) transcript refers to. Deliberately separated from
    /// executing that command: <see cref="Match"/> takes a string and returns which rule(s)
    /// matched (by stable <see cref="MatchResult.CommandLabel"/> plus any numeric parameter) --
    /// it never invokes anything, so it's testable by feeding it text with no microphone and no
    /// Unity scene. <see cref="Execute"/> is the separate step that actually invokes a matched
    /// rule's action, looked up by that same label.
    ///
    /// This split also keeps the door open for a future step (not implemented here): a small
    /// local LLM sitting between the normalized transcript and this class, interpreting looser
    /// phrasing into the same (label, param) shape and calling the same <see cref="Execute"/> --
    /// swapping in that interpreter would only ever touch what runs *before* Execute, never
    /// Execute itself or anything downstream of it. <see cref="AllCommandLabels"/> exists for
    /// that future interpreter's use (e.g. as the fixed set of commands it can target); it's not
    /// consumed by anything today.
    ///
    /// The rule table here is also the single source of truth for the app's voice vocabulary --
    /// every word/phrase a rule can trigger on is collected automatically as rules are built
    /// (see Rule()/ParamRule()) into <see cref="GrammarWords"/>/<see cref="GrammarPhrases"/>,
    /// which the Whisper sidecar's initial_prompt is built from. Add or rename a command here only.
    /// </summary>
    public class VoiceCommandMatcher
    {
        /// <summary>Result of matching one command segment -- a label identifying which rule
        /// fired, and its numeric parameter if the rule takes one. Never carries a delegate;
        /// look the label up via <see cref="Execute"/> to actually run it.</summary>
        public struct MatchResult
        {
            public string CommandLabel;
            public int? Param;
        }

        public class VoiceCommandRule
        {
            public string Label;
            // All groups must match (AND); any word within a group matches it (OR).
            public string[][] RequiredWordGroups;
            // If true, this rule is skipped when a number was heard anywhere in the
            // transcript -- for rules (the reset ladder) that should never compete
            // with a slice/zoom/rotate percentage command just because a mis-heard
            // word happens to also satisfy their word groups.
            public bool SkipIfNumberHeard;
            // Exactly one of these is set. ParamAction rules also require a number
            // (digit or spelled out, e.g. "twenty") somewhere in the transcript; if
            // none is found, the rule is skipped rather than treated as a match.
            public Action Action;
            public Action<int> ParamAction;
        }

        /// <summary>Fuzzy (edit-distance) matching on longer words can be toggled off to compare
        /// against a Whisper transcript's own error profile -- Whisper's error profile differs
        /// from keyword-grammar recognizers, so "correcting" an already-correct transcript
        /// could do more harm than good. Default on, matching existing behavior.</summary>
        public bool FuzzyMatchingEnabled = true;

        private List<VoiceCommandRule> rules;
        private readonly HashSet<string> grammarWords = new HashSet<string>();
        private readonly List<string> grammarPhrases = new List<string>();

        public IReadOnlyCollection<string> GrammarWords => grammarWords;
        public IReadOnlyList<string> GrammarPhrases => grammarPhrases;
        public IEnumerable<string> AllCommandLabels => rules.Select(r => r.Label).Distinct();

        public VoiceCommandMatcher(VoiceCommandActions actions)
        {
            BuildVocabulary(actions);
        }

        /// <summary>
        /// Splits a normalized transcript on "and"/"then" (capped at 3 segments, same as
        /// before) and matches each segment independently, in order. Returns one MatchResult per
        /// segment that matched something; segments that matched nothing are silently skipped
        /// (same behavior as today -- a filler-only segment just doesn't fire anything).
        /// </summary>
        public List<MatchResult> Match(string normalizedTranscript)
        {
            var results = new List<MatchResult>();
            if (string.IsNullOrWhiteSpace(normalizedTranscript))
                return results;

            string[] segments = Regex.Split(normalizedTranscript, @"\band\b|\bthen\b", RegexOptions.IgnoreCase)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Take(3)
                .ToArray();

            foreach (var segment in segments)
            {
                if (TryMatchOne(segment, out MatchResult result))
                    results.Add(result);
            }

            return results;
        }

        private bool TryMatchOne(string segment, out MatchResult result)
        {
            // Normalization already lowercases; this also tolerates being called directly with
            // un-normalized text (e.g. from a test) by not assuming case.
            string[] orderedWords = Regex.Matches(segment.ToLowerInvariant(), "[a-z']+|[0-9]+")
                .Select(m => m.Value).ToArray();
            var words = new HashSet<string>(orderedWords);

            foreach (var rule in rules)
            {
                if (!rule.RequiredWordGroups.All(group => group.Any(candidate => WordGroupContains(words, candidate))))
                    continue;

                if (rule.SkipIfNumberHeard && TryParseNumber(orderedWords, out _))
                    continue;

                if (rule.ParamAction != null)
                {
                    if (!TryParseNumber(orderedWords, out int value))
                        continue; // trigger words matched but no number heard -- keep looking
                    result = new MatchResult { CommandLabel = rule.Label, Param = value };
                    return true;
                }

                result = new MatchResult { CommandLabel = rule.Label, Param = null };
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>Invokes the rule identified by a match's label. The only place a
        /// VoiceCommandRule's Action/ParamAction is ever called.</summary>
        public void Execute(MatchResult match)
        {
            var rule = rules.FirstOrDefault(r => r.Label == match.CommandLabel);
            if (rule == null)
            {
                Debug.LogWarning($"[VoiceCommandMatcher] No rule registered for label \"{match.CommandLabel}\".");
                return;
            }

            if (rule.ParamAction != null && match.Param.HasValue)
                rule.ParamAction.Invoke(match.Param.Value);
            else
                rule.Action?.Invoke();
        }

        // ------------------------------------------------------------------
        // Number parsing -- shared by matching (numeric rules / SkipIfNumberHeard) and by
        // normalization's digit/word bridging upstream of this class.
        // ------------------------------------------------------------------

        // Number words a spoken command may use -- combined with digit-string parsing so a
        // Whisper transcript's own digit output ("40 percent") is understood equally.
        private static readonly Dictionary<string, int> NumberWords = new Dictionary<string, int>
        {
            { "zero", 0 }, { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 },
            { "five", 5 }, { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 },
            { "ten", 10 }, { "eleven", 11 }, { "twelve", 12 }, { "thirteen", 13 }, { "fourteen", 14 },
            { "fifteen", 15 }, { "sixteen", 16 }, { "seventeen", 17 }, { "eighteen", 18 }, { "nineteen", 19 },
            { "twenty", 20 }, { "thirty", 30 }, { "forty", 40 }, { "fifty", 50 },
            { "sixty", 60 }, { "seventy", 70 }, { "eighty", 80 }, { "ninety", 90 }, { "hundred", 100 },
        };

        // Finds the first run of number words/digits in the transcript and combines
        // them into a single value, e.g. "rotate by twenty five degrees" -> 25,
        // "zoom in one hundred percent" -> 100, "zoom in 40 percent" -> 40.
        private static bool TryParseNumber(string[] words, out int value)
        {
            bool found = false;
            int current = 0;

            foreach (var word in words)
            {
                if (int.TryParse(word, out int digit))
                {
                    current += digit;
                    found = true;
                    continue;
                }

                if (NumberWords.TryGetValue(word, out int wordValue))
                {
                    found = true;
                    current = wordValue == 100 ? Math.Max(current, 1) * 100 : current + wordValue;
                    continue;
                }

                if (found)
                    break; // number-word run ended
            }

            value = current;
            return found;
        }

        // Matches a vocabulary word against the heard words, tolerating small
        // mis-transcriptions (e.g. hearing "move" instead of "remove") via edit distance
        // instead of requiring a hand-maintained list of known confusions, unless
        // FuzzyMatchingEnabled is off. Short words (<=3 letters, e.g. "in"/"out") always
        // require an exact match -- fuzzy-matching common short words causes false
        // positives (e.g. "add" vs "and").
        private bool WordGroupContains(HashSet<string> heardWords, string candidate)
        {
            if (heardWords.Contains(candidate))
                return true;
            if (!FuzzyMatchingEnabled || candidate.Length <= 3)
                return false;

            int threshold = candidate.Length <= 5 ? 1 : 2;
            foreach (var heard in heardWords)
            {
                if (Math.Abs(heard.Length - candidate.Length) > threshold)
                    continue;
                if (LevenshteinDistance(heard, candidate) <= threshold)
                    return true;
            }
            return false;
        }

        private static int LevenshteinDistance(string a, string b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[a.Length, b.Length];
        }

        /// <summary>
        /// Writes the matcher's vocabulary out as plain JSON, so the Whisper sidecar can build its initial_prompt from the actual command vocabulary
        /// instead of a hand-maintained duplicate that would drift as commands are added here.
        /// Called once at startup; the sidecar re-reads the file itself, so it stays
        /// independently launchable/testable even without Unity having run first (it falls back
        /// to a small built-in default if the file isn't there yet).
        /// </summary>
        public void ExportVocabulary(string path)
        {
            try
            {
                string wordsJson = string.Join(", ", grammarWords.OrderBy(w => w).Select(w => "\"" + EscapeJson(w) + "\""));
                string phrasesJson = string.Join(", ", grammarPhrases.Distinct().Select(p => "\"" + EscapeJson(p) + "\""));
                string json = "{\n  \"words\": [" + wordsJson + "],\n  \"phrases\": [" + phrasesJson + "]\n}\n";
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[VoiceCommandMatcher] Failed to export vocabulary to " + path + ": " + e.Message);
            }
        }

        private static string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        // ------------------------------------------------------------------
        // Vocabulary -- add or rename presenter phrases here only. Every rule calls an
        // existing app function; none touch scene objects directly. Word groups are AND'ed;
        // words within a group are OR'd. Every word used in a group here is also automatically
        // added to the recognition grammar/prompt vocabulary -- nothing extra to update when
        // adding a new command below.
        // ------------------------------------------------------------------

        private void BuildVocabulary(VoiceCommandActions a)
        {
            rules = new List<VoiceCommandRule>();

            // Reset ladder, most specific first. "remove slice"/"reset view" must be checked
            // before the bare "reset" safety net below. All skip if a number was heard: a
            // spoken number strongly signals a slice/zoom/rotate percentage command instead.
            rules.Add(Rule("remove slice", new[] { new[] { "remove", "reset" }, new[] { "slice", "cross" } },
                a.RemoveSlice, skipIfNumberHeard: true));
            rules.Add(Rule("clear", new[] { new[] { "clear" } }, a.Clear, skipIfNumberHeard: true));
            rules.Add(Rule("reset view", new[] { new[] { "reset" }, new[] { "view", "camera" } },
                a.ResetView, skipIfNumberHeard: true));

            // Safety net: bare "reset"/"whole body"/"start over" catch anything not matched
            // above.
            rules.Add(Rule("reset", new[] { new[] { "reset" } }, a.ResetAll, skipIfNumberHeard: true));
            rules.Add(Rule("whole body", new[] { new[] { "whole" }, new[] { "body" } }, a.ResetAll, skipIfNumberHeard: true));
            rules.Add(Rule("start over", new[] { new[] { "start" }, new[] { "over" } }, a.ResetAll, skipIfNumberHeard: true));

            AddLayerRule("muscles", new[] { "muscle", "muscles" }, a.ShowMuscles, a.HideMuscles);
            AddLayerRule("skeleton", new[] { "skeleton", "bone", "bones" }, a.ShowSkeleton, a.HideSkeleton);
            AddLayerRule("nerves", new[] { "nerve", "nerves" }, a.ShowNerves, a.HideNerves);
            AddLayerRule("organs", new[] { "organ", "organs" }, a.ShowOrgans, a.HideOrgans);
            AddLayerRule("blood vessels", new[] { "vessel", "vessels", "artery", "arteries", "vein", "veins" },
                a.ShowBloodVessels, a.HideBloodVessels);
            AddLayerRule("lymph", new[] { "lymph", "lymphatic", "lymphatics" }, a.ShowLymph, a.HideLymph);
            AddLayerRule("joints", new[] { "joint", "joints" }, a.ShowJoints, a.HideJoints);

            AddHeroOrganRule("heart", a.ShowHeart, "heart");
            AddHeroOrganRule("brain", a.ShowBrain, "brain");
            AddHeroOrganRule("lungs", a.ShowLungs, "lung", "lungs");
            AddHeroOrganRule("liver", a.ShowLiver, "liver");
            AddHeroOrganRule("stomach", a.ShowStomach, "stomach");

            rules.Add(Rule("focus", new[] { new[] { "focus" } }, a.FocusOnLastShown));
            rules.Add(Rule("front", new[] { new[] { "front" } }, a.ResetOrientation));
            rules.Add(Rule("pan up", new[] { new[] { "pan" }, new[] { "up" } }, a.PanUp));
            rules.Add(Rule("pan down", new[] { new[] { "pan" }, new[] { "down" } }, a.PanDown));

            // Numbered zoom rules are checked first: if "zoom in/out" is heard with a number, it
            // wins; if the same trigger words are heard with no number, matching falls through
            // to the plain rules below.
            rules.Add(ParamRule("zoom in by percent", new[] { new[] { "zoom" }, new[] { "in" } },
                a.ZoomInByPercent,
                examplePhrases: new[] { "zoom in ten percent", "zoom in fifty percent" }));
            rules.Add(ParamRule("zoom out by percent", new[] { new[] { "zoom" }, new[] { "out" } },
                a.ZoomOutByPercent,
                examplePhrases: new[] { "zoom out ten percent", "zoom out fifty percent" }));
            rules.Add(Rule("zoom in", new[] { new[] { "zoom" }, new[] { "in" } }, a.FocusOnLastShown));
            rules.Add(Rule("zoom out", new[] { new[] { "zoom" }, new[] { "out" } }, a.ZoomOut));

            rules.Add(ParamRule("rotate by degrees", new[] { new[] { "rotate", "turn", "spin" } },
                a.RotateByDegrees,
                examplePhrases: new[] { "rotate by twenty degrees", "rotate by ninety degrees", "turn by forty five degrees", "spin by thirty degrees" }));

            // "slow rotate" must be checked before the plain "rotate" rule.
            rules.Add(Rule("slow rotate", new[] { new[] { "slow" }, new[] { "rotate", "turn", "spin" } }, a.SlowRotate));
            rules.Add(Rule("rotate", new[] { new[] { "rotate", "turn", "spin" } }, a.Rotate));

            // Cross-section (front-to-back slice). 0% = front, 100% = back. "slice at N"
            // (absolute) is checked before plain "slice N" (relative).
            rules.Add(ParamRule("slice to percent", new[] { new[] { "slice", "cross" }, new[] { "at" } },
                a.SliceToPercent,
                examplePhrases: new[] { "slice at forty percent", "slice at ten percent", "cross at seventy percent" }));
            rules.Add(ParamRule("slice deeper by percent", new[] { new[] { "slice", "cross" } },
                a.SliceDeeperByPercent,
                examplePhrases: new[] { "slice twenty percent", "slice ten percent", "cross twenty percent" }));
            rules.Add(Rule("slice back", new[] { new[] { "slice", "cross" }, new[] { "back" } }, a.SliceBack));
            rules.Add(Rule("slow slice", new[] { new[] { "slow" }, new[] { "slice", "cross" } }, a.SlowSlice));
            rules.Add(Rule("slice on", new[] { new[] { "slice", "cross" } }, a.SliceOn));

            // Hand-tracking backend switch -- voice-triggered, calls into HandInputCoordinator
            // (not the Leap/gesture code itself).
            rules.Add(Rule("gesture off", new[] { new[] { "gesture", "gestures" }, new[] { "off", "disable" } }, a.GestureOff));
            rules.Add(Rule("gesture leap", new[] { new[] { "gesture", "gestures" }, new[] { "leap" } }, a.GestureLeap));
            rules.Add(Rule("gesture video", new[] { new[] { "gesture", "gestures" }, new[] { "video", "camera" } }, a.GestureVideo));

            // Words that don't belong to any single rule's RequiredWordGroups but the
            // recognizer/prompt still needs to know about: spelled-out numbers, and the
            // compound-command connectors.
            foreach (var numberWord in NumberWords.Keys)
                grammarPhrases.Add(numberWord);
            grammarWords.UnionWith(NumberWords.Keys);
            grammarPhrases.Add("and");
            grammarPhrases.Add("then");
            grammarWords.Add("and");
            grammarWords.Add("then");
        }

        private void AddLayerRule(string label, string[] subjectWords, Action show, Action hide)
        {
            rules.Add(Rule("show " + label, new[] { new[] { "show", "add" }, subjectWords }, show));
            rules.Add(Rule("remove " + label, new[] { new[] { "remove", "hide" }, subjectWords }, hide));
        }

        private void AddHeroOrganRule(string label, Action show, params string[] subjectWords)
        {
            rules.Add(Rule("show " + label, new[] { new[] { "show", "add" }, subjectWords }, show));
        }

        private VoiceCommandRule Rule(string label, string[][] groups, Action action, bool skipIfNumberHeard = false)
        {
            RegisterGrammarWords(groups);
            return new VoiceCommandRule { Label = label, RequiredWordGroups = groups, Action = action, SkipIfNumberHeard = skipIfNumberHeard };
        }

        private VoiceCommandRule ParamRule(string label, string[][] groups, Action<int> action, string[] examplePhrases = null)
        {
            RegisterGrammarWords(groups);
            if (examplePhrases != null)
                grammarPhrases.AddRange(examplePhrases);
            return new VoiceCommandRule { Label = label, RequiredWordGroups = groups, ParamAction = action };
        }

        private void RegisterGrammarWords(string[][] groups)
        {
            foreach (var group in groups)
                foreach (var word in group)
                    grammarWords.Add(word);
            AddPhraseCombinations(groups);
        }

        private void AddPhraseCombinations(string[][] groups)
        {
            IEnumerable<string> phrases = new[] { "" };
            foreach (var group in groups)
            {
                phrases = phrases.SelectMany(prefix =>
                    group.Select(word => string.IsNullOrEmpty(prefix) ? word : prefix + " " + word));
            }
            grammarPhrases.AddRange(phrases);
        }
    }
}
