using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Scripts.Scripting
{
    public class ParseError
    {
        public int Line;
        public string Text;
        public string Message;

        public override string ToString() => Line > 0 ? $"line {Line}: {Message}" : Message;
    }

    public class ParseResult
    {
        public List<CommandInvocation> Commands = new List<CommandInvocation>();
        public List<ParseError> Errors = new List<ParseError>();
    }

    /// <summary>
    /// The strict parser for typed commands and macro files (speech gets mapped onto the same text by the
    /// voice bridge first). Pure text in, <see cref="CommandInvocation"/>s out -- no scene access, so it's
    /// testable in EditMode.
    ///
    /// Syntax: one command per line; ";", "then" and "and" also separate commands on a line. A segment that
    /// doesn't start with a verb (or a macro name) joins the previous command's argument after "and"/"then",
    /// so "show heart and lungs" is one show. Leading line numbers ("1 reset") are ignored, "#" starts a
    /// comment, "quoted text" is passed through unchanged, number words become digits, and a trailing
    /// "&amp;" runs the command in parallel.
    /// </summary>
    public static class CommandParser
    {
        private static readonly Regex LeadingLineNumber = new Regex(@"^\s*\d+[.):]?\s+(?=[A-Za-z])");
        private static readonly Regex LeadingWord = new Regex("\"|[^\\s\"]+");
        private static readonly Regex ArgToken = new Regex("\"([^\"]*)(?:\"|$)|[^\\s,\"]+");

        private const int MaxVerbWords = 3;

        public static ParseResult Parse(string text)
        {
            var result = new ParseResult();
            if (string.IsNullOrWhiteSpace(text))
                return result;

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = StripComment(lines[i].TrimEnd('\r'));
                line = LeadingLineNumber.Replace(line, "", 1);
                if (!string.IsNullOrWhiteSpace(line))
                    ParseLine(line, i + 1, result);
            }
            return result;
        }

        private static void ParseLine(string line, int lineNumber, ParseResult result)
        {
            var lineCommands = new List<CommandInvocation>();

            foreach (var segment in SplitSegments(line))
            {
                string text = segment.Text.Trim();
                bool parallel = false;
                if (text.EndsWith("&"))
                {
                    parallel = true;
                    text = text.Substring(0, text.Length - 1).TrimEnd();
                }
                if (text.Length == 0)
                    continue;

                var invocation = Bind(CommandRegistry.ApplyPhrases(text));
                if (invocation == null)
                {
                    // "show heart and lungs": "lungs" isn't a verb, so it extends the previous command.
                    if (segment.Connector != null && lineCommands.Count > 0)
                    {
                        var previous = lineCommands[lineCommands.Count - 1];
                        previous.Args = (previous.Args + " " + segment.Connector + " " + text).Trim();
                        previous.Parallel |= parallel;
                        Tokenize(previous);
                        continue;
                    }
                    result.Errors.Add(new ParseError { Line = lineNumber, Text = text, Message = UnknownCommandMessage(text) });
                    continue;
                }

                invocation.Parallel = parallel;
                invocation.Line = lineNumber;
                lineCommands.Add(invocation);
            }

            // Validated only once the whole line is read, since a later "and ..." segment can still add to
            // an earlier command's argument.
            foreach (var invocation in lineCommands)
            {
                string error = invocation.Command.Validate(invocation);
                if (error != null)
                    result.Errors.Add(new ParseError { Line = lineNumber, Text = invocation.ToString(), Message = $"{invocation}: {error}" });
                else
                    result.Commands.Add(invocation);
            }
        }

        /// <summary>Matches a verb (longest first) or, failing that, a macro name. Null if neither.</summary>
        private static CommandInvocation Bind(string text)
        {
            var words = new List<string>();
            var wordEnds = new List<int>();
            foreach (Match match in LeadingWord.Matches(text))
            {
                if (match.Value == "\"" || words.Count == MaxVerbWords)
                    break;
                words.Add(match.Value.TrimEnd(',').ToLowerInvariant());
                wordEnds.Add(match.Index + match.Length);
            }

            CommandInvocation invocation;
            if (CommandRegistry.TryMatchVerb(words, out var command, out int verbWords))
            {
                invocation = new CommandInvocation { Command = command, Verb = command.Info.Verb, Args = text.Substring(wordEnds[verbWords - 1]).Trim() };
            }
            else if (MacroLibrary.TryMatch(text, out _, out _) && CommandRegistry.Find("run") is AnatomyCommand run)
            {
                // A macro's own name runs it: "heart tour" == "run heart tour", and a template macro is
                // called like a verb: "explain aorta" == "run explain aorta".
                invocation = new CommandInvocation { Command = run, Verb = run.Info.Verb, Args = text };
            }
            else
            {
                return null;
            }

            Tokenize(invocation);
            return invocation;
        }

        private static void Tokenize(CommandInvocation invocation)
        {
            invocation.Words.Clear();
            invocation.Quoted.Clear();

            var raw = new List<string>();
            foreach (Match match in ArgToken.Matches(invocation.Args))
            {
                if (match.Value.StartsWith("\""))
                {
                    invocation.Quoted.Add(match.Groups[1].Value);
                    continue;
                }
                string token = match.Value.ToLowerInvariant().TrimEnd('%');
                // "twenty-five" -> "twenty", "five" so the number folding below sees both parts.
                if (token.Contains('-') && token.Split('-').All(part => NumberWords.ContainsKey(part)))
                    raw.AddRange(token.Split('-'));
                else if (token.Length > 0)
                    raw.Add(token);
            }
            invocation.Words.AddRange(FoldNumberWords(raw));
        }

        // ------------------------------------------------------------------
        // Text helpers
        // ------------------------------------------------------------------

        private struct Segment
        {
            public string Text;
            /// <summary>"and"/"then" if that word introduced this segment (it may then join the previous
            /// command); null after ";" or at the start of a line.</summary>
            public string Connector;
        }

        private static List<Segment> SplitSegments(string line)
        {
            var segments = new List<Segment>();
            var current = new StringBuilder();
            string connector = null;
            bool inQuote = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                    inQuote = !inQuote;

                if (!inQuote)
                {
                    if (c == ';')
                    {
                        segments.Add(new Segment { Text = current.ToString(), Connector = connector });
                        current.Clear();
                        connector = null;
                        continue;
                    }

                    string word = ConnectorAt(line, i);
                    if (word != null)
                    {
                        segments.Add(new Segment { Text = current.ToString(), Connector = connector });
                        current.Clear();
                        connector = word;
                        i += word.Length - 1;
                        continue;
                    }
                }
                current.Append(c);
            }
            segments.Add(new Segment { Text = current.ToString(), Connector = connector });
            return segments;
        }

        // "and"/"then" as a whole word starting at index i, or null.
        private static string ConnectorAt(string line, int i)
        {
            if (i > 0 && !char.IsWhiteSpace(line[i - 1]))
                return null;
            foreach (var word in new[] { "and", "then" })
            {
                if (string.Compare(line, i, word, 0, word.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    continue;
                int end = i + word.Length;
                if (end == line.Length || char.IsWhiteSpace(line[end]))
                    return word;
            }
            return null;
        }

        private static string StripComment(string line)
        {
            bool inQuote = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                    inQuote = !inQuote;
                else if (line[i] == '#' && !inQuote)
                    return line.Substring(0, i);
            }
            return line;
        }

        /// <summary>Splits an argument into list items on commas and "and", outside quotes; quotes are stripped.</summary>
        public static List<string> SplitItems(string args)
        {
            var items = new List<string>();
            var current = new StringBuilder();
            bool inQuote = false;

            void Flush()
            {
                string item = current.ToString().Trim();
                if (item.Length > 0)
                    items.Add(item);
                current.Clear();
            }

            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];
                if (c == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }
                if (!inQuote && c == ',')
                {
                    Flush();
                    continue;
                }
                if (!inQuote && ConnectorAt(args, i) == "and")
                {
                    Flush();
                    i += 2;
                    continue;
                }
                current.Append(c);
            }
            Flush();
            return items;
        }

        private static string UnknownCommandMessage(string text)
        {
            string first = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant() ?? text;
            string closest = CommandRegistry.AllNames
                .Where(name => !name.Contains(' '))
                .Select(name => new { name, distance = Levenshtein(first, name) })
                .Where(x => x.distance <= Math.Max(1, first.Length / 3))
                .OrderBy(x => x.distance)
                .Select(x => x.name)
                .FirstOrDefault();
            return closest != null
                ? $"unknown command \"{text}\" -- did you mean \"{closest}\"?"
                : $"unknown command \"{text}\" (type help for the list)";
        }

        internal static int Levenshtein(string a, string b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            return d[a.Length, b.Length];
        }

        // ------------------------------------------------------------------
        // Number words -- same values the voice matcher accepts, so "slice at forty" types and speaks alike.
        // ------------------------------------------------------------------

        private static readonly Dictionary<string, int> NumberWords = new Dictionary<string, int>
        {
            { "zero", 0 }, { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 },
            { "five", 5 }, { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 },
            { "ten", 10 }, { "eleven", 11 }, { "twelve", 12 }, { "thirteen", 13 }, { "fourteen", 14 },
            { "fifteen", 15 }, { "sixteen", 16 }, { "seventeen", 17 }, { "eighteen", 18 }, { "nineteen", 19 },
            { "twenty", 20 }, { "thirty", 30 }, { "forty", 40 }, { "fifty", 50 },
            { "sixty", 60 }, { "seventy", 70 }, { "eighty", 80 }, { "ninety", 90 }, { "hundred", 100 },
        };

        // Each run of number words becomes one digit token: "twenty five degrees" -> "25 degrees",
        // "one hundred" -> "100".
        private static IEnumerable<string> FoldNumberWords(List<string> tokens)
        {
            int? run = null;
            foreach (var token in tokens)
            {
                if (NumberWords.TryGetValue(token, out int value))
                {
                    int current = run ?? 0;
                    run = value == 100 ? Math.Max(current, 1) * 100 : current + value;
                    continue;
                }
                if (run.HasValue)
                {
                    yield return run.Value.ToString();
                    run = null;
                }
                yield return token;
            }
            if (run.HasValue)
                yield return run.Value.ToString();
        }
    }
}
