using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// Finds every <see cref="AnatomyCommand"/> marked with <see cref="CommandAttribute"/> by reflection,
    /// once, and answers "which verb does this text start with". Verb names, aliases and phrase rewrites
    /// all come from the attributes; nothing here lists commands by hand.
    /// </summary>
    public static class CommandRegistry
    {
        // Longest verb/alias is "start over"-style two or three words; matching tries the longest first
        // so "zoom in" can never be shadowed by a shorter "zoom".
        private const int MaxVerbWords = 3;

        private static List<AnatomyCommand> commands;
        private static Dictionary<string, AnatomyCommand> byName;
        private static List<KeyValuePair<string, string>> phrases;

        public static IReadOnlyList<AnatomyCommand> All
        {
            get { EnsureBuilt(); return commands; }
        }

        /// <summary>Every verb name and alias, for tab completion and voice vocabulary.</summary>
        public static IEnumerable<string> AllNames
        {
            get { EnsureBuilt(); return byName.Keys; }
        }

        public static AnatomyCommand Find(string name)
        {
            EnsureBuilt();
            byName.TryGetValue(name.Trim().ToLowerInvariant(), out var command);
            return command;
        }

        /// <summary>Matches the longest verb name/alias at the start of <paramref name="leadingWords"/>
        /// (already lowercased). Returns how many words the verb used.</summary>
        public static bool TryMatchVerb(IList<string> leadingWords, out AnatomyCommand command, out int wordCount)
        {
            EnsureBuilt();
            for (int n = Math.Min(MaxVerbWords, leadingWords.Count); n >= 1; n--)
            {
                string candidate = string.Join(" ", leadingWords.Take(n));
                if (byName.TryGetValue(candidate, out command))
                {
                    wordCount = n;
                    return true;
                }
            }
            command = null;
            wordCount = 0;
            return false;
        }

        /// <summary>Rewrites a leading phrase ("slow rotate ..." -> "rotate slow ..."), longest phrase
        /// first, matching whole words case-insensitively. Text after the phrase is left untouched.</summary>
        public static string ApplyPhrases(string segment)
        {
            EnsureBuilt();
            string trimmed = segment.TrimStart();
            foreach (var phrase in phrases)
            {
                if (!trimmed.StartsWith(phrase.Key, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (trimmed.Length > phrase.Key.Length && !char.IsWhiteSpace(trimmed[phrase.Key.Length]))
                    continue; // "front" must not rewrite "frontal ..."
                return phrase.Value + trimmed.Substring(phrase.Key.Length);
            }
            return segment;
        }

        private static void EnsureBuilt()
        {
            if (commands != null)
                return;

            commands = new List<AnatomyCommand>();
            byName = new Dictionary<string, AnatomyCommand>();
            phrases = new List<KeyValuePair<string, string>>();

            var types = typeof(AnatomyCommand).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(AnatomyCommand).IsAssignableFrom(t));

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<CommandAttribute>();
                if (attribute == null)
                {
                    Debug.LogWarning($"[CommandRegistry] {type.Name} has no [Command] attribute -- skipped.");
                    continue;
                }

                var command = (AnatomyCommand)Activator.CreateInstance(type);
                command.Info = attribute;
                commands.Add(command);

                foreach (var name in new[] { attribute.Verb }.Concat(attribute.Aliases))
                    Register(name, command);

                foreach (var phrase in attribute.Phrases)
                {
                    int arrow = phrase.IndexOf("=>", StringComparison.Ordinal);
                    if (arrow < 0)
                    {
                        Debug.LogWarning($"[CommandRegistry] Phrase \"{phrase}\" on {type.Name} has no \"=>\" -- skipped.");
                        continue;
                    }
                    phrases.Add(new KeyValuePair<string, string>(phrase.Substring(0, arrow).Trim(), phrase.Substring(arrow + 2).Trim()));
                }
            }

            commands.Sort((a, b) => string.CompareOrdinal(a.Info.Verb, b.Info.Verb));
            phrases.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
        }

        private static void Register(string name, AnatomyCommand command)
        {
            string key = name.Trim().ToLowerInvariant();
            if (byName.TryGetValue(key, out var existing))
            {
                Debug.LogWarning($"[CommandRegistry] \"{key}\" is claimed by both {existing.GetType().Name} and {command.GetType().Name}; keeping {existing.GetType().Name}.");
                return;
            }
            byName[key] = command;
        }
    }
}
