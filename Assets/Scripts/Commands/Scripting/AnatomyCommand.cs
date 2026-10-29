using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// Marks an <see cref="AnatomyCommand"/> subclass for <see cref="CommandRegistry"/>. This one
    /// attribute is the single source of truth for a verb: its spoken/typed names, console help and
    /// tab completion all come from here, so adding a command never means editing a central list.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Verb { get; }
        /// <summary>Other names for the same verb, e.g. "turn"/"spin" for rotate. May be multi-word.</summary>
        public string[] Aliases { get; set; } = new string[0];
        /// <summary>Phrase rewrites applied before verb lookup, written "from => to", e.g.
        /// "slow rotate => rotate slow". For phrasings that carry an argument, which a plain alias can't.</summary>
        public string[] Phrases { get; set; } = new string[0];
        /// <summary>Argument summary for help, e.g. "[slow | &lt;degrees&gt;]".</summary>
        public string Args { get; set; } = "";
        public string Help { get; set; } = "";

        public CommandAttribute(string verb)
        {
            Verb = verb;
        }
    }

    /// <summary>
    /// One verb of the command language. Instances are stateless singletons created by
    /// <see cref="CommandRegistry"/>; everything about a particular call lives in the
    /// <see cref="CommandContext"/>. <see cref="Execute"/> returns an IEnumerator the
    /// <see cref="CommandRunner"/> yields on, so a command that animates finishes before the next starts.
    /// </summary>
    public abstract class AnatomyCommand
    {
        public CommandAttribute Info { get; internal set; }

        /// <summary>Checks arguments at parse time, so a macro reports bad lines when it loads rather than
        /// halfway through a lecture. Returns an error message, or null if the invocation is valid.</summary>
        public virtual string Validate(CommandInvocation invocation) => null;

        public abstract IEnumerator Execute(CommandContext ctx);

        /// <summary>Usage line for help: verb plus argument summary.</summary>
        public string Usage => string.IsNullOrEmpty(Info.Args) ? Info.Verb : Info.Verb + " " + Info.Args;

        // ------------------------------------------------------------------
        // Waiting helpers -- shared by every command that starts an animation.
        // ------------------------------------------------------------------

        // Safety net only: every animation these wait on ends by itself, but a command must never be
        // able to hang a macro if some future animation doesn't.
        private const float MaxAnimationWaitSeconds = 30f;

        protected static IEnumerator WaitWhile(Func<bool> busy)
        {
            float start = Time.time;
            while (busy() && Time.time - start < MaxAnimationWaitSeconds)
                yield return null;
        }

        protected static IEnumerator WaitForCamera() => WaitWhile(() => CameraController.instance != null && CameraController.instance.IsAnimating);
    }

    public enum CommandSource { Console, Voice, Macro }

    /// <summary>Everything a command needs to know about one call of it.</summary>
    public class CommandContext
    {
        public CommandRunner Runner;
        public CommandInvocation Invocation;
        /// <summary>Macro nesting depth; 0 for a command typed or spoken directly.</summary>
        public int Depth;
        public CommandSource Source;

        public CommandInvocation Inv => Invocation;
    }

    /// <summary>
    /// A parsed command: which verb, its argument text, and that text tokenized. Produced by
    /// <see cref="CommandParser"/> from console input, voice (via the voice bridge) and macro files alike.
    /// </summary>
    public class CommandInvocation
    {
        public AnatomyCommand Command;
        /// <summary>The verb's canonical name (not the alias that was typed).</summary>
        public string Verb;
        /// <summary>Argument text after the verb, original casing, trimmed.</summary>
        public string Args = "";
        /// <summary>Unquoted argument tokens, lowercased, with number words folded into digits ("forty" -> "40").</summary>
        public List<string> Words = new List<string>();
        /// <summary>Quoted strings in the arguments, unchanged.</summary>
        public List<string> Quoted = new List<string>();
        /// <summary>Trailing &amp;: start this command and move straight on instead of waiting for it.</summary>
        public bool Parallel;
        /// <summary>1-based source line, for macro error messages.</summary>
        public int Line;

        public bool HasArgs => Words.Count > 0 || Quoted.Count > 0;

        public bool Has(string word) => Words.Contains(word);

        public bool HasAny(params string[] words) => words.Any(Words.Contains);

        /// <summary>First numeric token, e.g. 40 from "at 40 percent".</summary>
        public bool TryGetNumber(out float value)
        {
            foreach (var word in Words)
            {
                if (float.TryParse(word, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    return true;
            }
            value = 0;
            return false;
        }

        /// <summary>The argument split into list items on commas and "and", e.g. "heart, aorta and lungs"
        /// -> [heart, aorta, lungs]. Quoted text is never split.</summary>
        public List<string> Items => CommandParser.SplitItems(Args);

        public override string ToString()
        {
            string text = string.IsNullOrEmpty(Args) ? Verb : Verb + " " + Args;
            return Parallel ? text + " &" : text;
        }
    }
}
