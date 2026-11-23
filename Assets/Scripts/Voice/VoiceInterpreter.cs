using Assets.Scripts.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// The voice bridge: turns a transcript into canonical command text for <see cref="CommandRunner"/>,
    /// so speech, the console and macros all execute through the same commands. Recognition itself is
    /// unchanged -- <see cref="VoiceCommandMatcher"/> still decides which rule a phrase hit -- and this
    /// only maps each rule's label onto the command it means. Logs every step (raw -> normalized -> command)
    /// to the console so a phrase that didn't work can be diagnosed.
    ///
    /// This is also where a later "verb + part name" interpreter or a local LLM slots in: anything that
    /// produces command text here needs no other change.
    /// </summary>
    public static class VoiceInterpreter
    {
        private static VoiceCommandMatcher matcher;

        public static VoiceCommandMatcher Matcher
        {
            get
            {
                if (matcher == null)
                {
                    var config = AsrConfig.Load(Path.Combine(Application.streamingAssetsPath, "asr_config.json"));
                    matcher = new VoiceCommandMatcher { FuzzyMatchingEnabled = config.FuzzyMatchingEnabled };
                }
                return matcher;
            }
        }

        // Matcher labels whose command text differs from the label itself. Every other label already is
        // the canonical command ("show heart", "focus", "slice back", ...); "remove X" labels become "hide X".
        private static readonly Dictionary<string, string> LabelToCommand = new Dictionary<string, string>
        {
            { "remove slice", "slice off" },
            { "whole body", "reset" },
            { "start over", "reset" },
            { "front", "view front" },
            { "zoom in by percent", "zoom in {n}" },
            { "zoom out by percent", "zoom out {n}" },
            { "rotate by degrees", "rotate {n}" },
            { "slow rotate", "rotate slow" },
            { "slice to percent", "slice at {n}" },
            { "slice deeper by percent", "slice {n}" },
            { "slow slice", "slice slow" },
        };

        /// <summary>Raw transcript in, command text out (several commands joined by "; "), or null if
        /// nothing matched.</summary>
        public static string Interpret(string rawTranscript)
        {
            string normalized = TranscriptNormalizer.Normalize(rawTranscript ?? "");
            CommandLog.Trace($"heard \"{rawTranscript}\" -> \"{normalized}\"");

            // "macro <name>" is taken out before rule matching: everything after the word is the name,
            // and its words ("heart", "slice", "and") mustn't fire commands of their own. Anything said
            // before it still matches normally, so "reset and macro heart intro" works.
            string macroCommand = null;
            if (TrySplitMacro(normalized, out string before, out string spokenName))
            {
                normalized = before;
                macroCommand = ResolveMacro(spokenName);
            }

            var commands = Matcher.Match(normalized).Select(ToCommandText).ToList();
            if (macroCommand != null)
                commands.Add(macroCommand);
            if (commands.Count == 0)
            {
                CommandLog.Trace("no command matched");
                return null;
            }

            string commandText = string.Join("; ", commands);
            CommandLog.Trace("-> " + commandText);
            return commandText;
        }

        private static readonly Regex SpokenWord = new Regex("[a-z']+|[0-9]+");

        // Finds the first "macro" (or a near miss like "micro"/"macros" when fuzzy matching is on) and
        // splits the transcript around it.
        private static bool TrySplitMacro(string normalized, out string before, out string spokenName)
        {
            foreach (Match word in SpokenWord.Matches(normalized))
            {
                bool isMacroWord = word.Value == VoiceCommandMatcher.MacroWord
                    || (Matcher.FuzzyMatchingEnabled && word.Value.Length >= 4
                        && VoiceCommandMatcher.LevenshteinDistance(word.Value, VoiceCommandMatcher.MacroWord) <= 1);
                if (!isMacroWord)
                    continue;

                before = normalized.Substring(0, word.Index);
                var nameWords = SpokenWord.Matches(normalized.Substring(word.Index + word.Length)).Cast<Match>().Select(m => m.Value);
                spokenName = string.Join(" ", nameWords);
                return true;
            }

            before = normalized;
            spokenName = null;
            return false;
        }

        /// <summary>"heart intro" -> "run heart intro". Exact names first, then the closest macro name
        /// by edit distance, so a mis-heard "hard intro" still finds heart_intro.macro. Name matching is
        /// always fuzzy: it only ever picks from the macro files that exist, so a near miss can't
        /// trigger anything else. A template macro keeps the words after its name as its part.</summary>
        private static string ResolveMacro(string spokenName)
        {
            if (spokenName.Length == 0)
            {
                CommandLog.Warning("say which macro after \"macro\", e.g. \"macro heart intro\"");
                return null;
            }

            // A pause mid-name can arrive as a comma, which normalization turns into "and".
            var candidates = new[] { spokenName, Regex.Replace(spokenName, @"\s*\band\b\s*", " ").Trim() }.Distinct();

            foreach (var candidate in candidates)
            {
                if (MacroLibrary.TryMatch(candidate, out _, out _))
                    return "run " + candidate;
            }

            string bestName = null, bestArgument = null;
            int bestDistance = int.MaxValue;
            foreach (var candidate in candidates)
            {
                string[] words = candidate.Split(' ');
                foreach (var name in MacroLibrary.Names)
                {
                    int nameWords = name.Split(' ').Length;
                    if (words.Length < nameWords)
                        continue;
                    int distance = VoiceCommandMatcher.LevenshteinDistance(string.Join(" ", words.Take(nameWords)), name);
                    if (distance > Math.Max(1, name.Length / 4))
                        continue;
                    string argument = string.Join(" ", words.Skip(nameWords));
                    if (argument.Length > 0 && !MacroLibrary.IsTemplate(name))
                        continue;
                    if (distance < bestDistance || (distance == bestDistance && name.Length > bestName.Length))
                    {
                        bestName = name;
                        bestArgument = argument;
                        bestDistance = distance;
                    }
                }
            }

            if (bestName == null)
            {
                CommandLog.Warning($"no macro sounds like \"{spokenName}\" -- macros: {string.Join(", ", MacroLibrary.Names)}");
                return null;
            }

            CommandLog.Trace($"macro \"{spokenName}\" -> {bestName}");
            return "run " + bestName + (bestArgument.Length > 0 ? " " + bestArgument : "");
        }

        private static string ToCommandText(VoiceCommandMatcher.MatchResult match)
        {
            string label = match.CommandLabel;
            string text;
            if (LabelToCommand.TryGetValue(label, out string mapped))
                text = mapped;
            else if (label.StartsWith("remove "))
                text = "hide " + label.Substring("remove ".Length);
            else
                text = label;

            string number = match.Param?.ToString() ?? "";
            return text.Contains("{n}") ? text.Replace("{n}", number) : text;
        }
    }
}
