using Assets.Scripts.Scripting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var matches = Matcher.Match(normalized);
            if (matches.Count == 0)
            {
                CommandLog.Trace("no command matched");
                return null;
            }

            string commandText = string.Join("; ", matches.Select(ToCommandText));
            CommandLog.Trace("-> " + commandText);
            return commandText;
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
