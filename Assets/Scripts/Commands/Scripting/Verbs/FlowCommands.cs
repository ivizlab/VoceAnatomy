using Assets.Scripts.Voice;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    [Command("wait", Args = "<seconds> | click | key",
        Help = "Pause a sequence: wait 3, or wait click / wait key until the presenter clicks the model or presses →, Page Down or Enter (what clickers send)")]
    public class WaitCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv)
        {
            if (inv.HasAny("click", "key", "next"))
                return null;
            return inv.TryGetNumber(out float seconds) && seconds >= 0 ? null : "needs seconds, or click/key, e.g. wait 3 or wait click";
        }

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (!ctx.Inv.HasAny("click", "key", "next"))
            {
                ctx.Inv.TryGetNumber(out float seconds);
                yield return new WaitForSeconds(seconds);
                yield break;
            }

            // "wait key" ignores clicks, so the presenter can still click parts while talking.
            bool clicksAdvance = !ctx.Inv.Has("key");
            if (clicksAdvance)
                PresenterInput.HoldClicks();
            CaptionPanel.Instance.SetWaitHint(true);

            // Skip a frame so the key or click that started this command can't also end it.
            yield return null;
            while (!PresenterInput.AdvanceKeyPressed && !(clicksAdvance && PresenterInput.ClickedModel))
                yield return null;

            CaptionPanel.Instance.SetWaitHint(false);
            if (clicksAdvance)
                PresenterInput.ReleaseClicks();
        }
    }

    [Command("stop", Help = "Cancel the running macro or sequence and any animation")]
    public class StopCmd : AnatomyCommand
    {
        public override IEnumerator Execute(CommandContext ctx)
        {
            ctx.Runner.Stop();
            CommandLog.Info("stopped");
            yield break;
        }
    }

    /// <summary>Loading and checking macro files, shared by run, check and the startup check.</summary>
    public static class MacroCheck
    {
        // Template macros are checked with this part filled in.
        private const string SamplePart = "heart";

        /// <summary>Why "name [argument]" can't run, or null if it can.</summary>
        public static string CallProblem(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "say which macro (type macros for the list)";
            if (!MacroLibrary.TryMatch(text, out string name, out string argument))
                return $"no macro named \"{text}\" in StreamingAssets/macros";
            bool template = MacroLibrary.IsTemplate(name);
            if (template && argument.Length == 0)
                return $"\"{name}\" is a template -- give it a part, e.g. {name} heart";
            if (!template && argument.Length > 0)
                return $"macro \"{name}\" doesn't take a part (\"{argument}\")";
            return template ? PartTargets.Validate(CommandParser.SplitItems(argument), allowEmpty: false) : null;
        }

        /// <summary>Parses a macro with {part} filled in. Null if the file can't be read.</summary>
        public static ParseResult Load(string name, string argument)
        {
            if (!MacroLibrary.TryRead(name, out string text, out _))
                return null;
            return CommandParser.Parse(MacroLibrary.Expand(text, argument));
        }

        /// <summary>Every problem in a macro file, with line numbers; empty if it's clean.</summary>
        public static List<string> Problems(string name)
        {
            var parsed = Load(name, MacroLibrary.IsTemplate(name) ? SamplePart : null);
            if (parsed == null)
                return new List<string> { "could not be read" };
            return parsed.Errors.Select(e => e.ToString()).ToList();
        }

        /// <summary>Checks every macro and logs the problems; returns how many macros have any.</summary>
        public static int CheckAll(bool reportClean)
        {
            int bad = 0;
            foreach (var name in MacroLibrary.Names)
            {
                var problems = Problems(name);
                if (problems.Count == 0)
                {
                    if (reportClean)
                        CommandLog.Info($"{name}: OK");
                    continue;
                }
                bad++;
                foreach (var problem in problems)
                    CommandLog.Warning($"macro {name}, {problem}");
            }
            return bad;
        }
    }

    [Command("run", Args = "<macro> [part]",
        Help = "Run a macro file from StreamingAssets/macros. Typing just its name also works; a template macro (one using {part}) takes a part: explain aorta")]
    public class RunCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => MacroCheck.CallProblem(inv.Args);

        public override IEnumerator Execute(CommandContext ctx)
        {
            MacroLibrary.TryMatch(ctx.Inv.Args, out string name, out string argument);
            if (ctx.Depth >= CommandRunner.MaxMacroDepth)
            {
                CommandLog.Error($"macro \"{name}\" not run: macros nested more than {CommandRunner.MaxMacroDepth} deep (does it run itself?)");
                yield break;
            }

            var parsed = MacroCheck.Load(name, argument);
            if (parsed == null)
            {
                CommandLog.Error($"macro \"{name}\" could not be read");
                yield break;
            }
            foreach (var error in parsed.Errors)
                CommandLog.Error($"{name}, {error} -- skipped");

            yield return ctx.Runner.RunList(parsed.Commands, ctx.Depth + 1, CommandSource.Macro);
        }
    }

    [Command("macros", Help = "List the available macros; templates show the part they take")]
    public class MacrosCmd : AnatomyCommand
    {
        public override IEnumerator Execute(CommandContext ctx)
        {
            var names = MacroLibrary.Names.Select(n => MacroLibrary.IsTemplate(n) ? n + " <part>" : n).ToList();
            CommandLog.Info(names.Count == 0
                ? $"no macros yet -- add .macro files to {MacroLibrary.Folder}"
                : "macros: " + string.Join(", ", names));
            yield break;
        }
    }

    [Command("check", Args = "[macro]", Help = "Check one macro, or all of them, for unknown commands and part names, with line numbers")]
    public class CheckCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) =>
            !inv.HasArgs || MacroLibrary.Exists(inv.Args) ? null : $"no macro named \"{inv.Args}\"";

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (!ctx.Inv.HasArgs)
            {
                int bad = MacroCheck.CheckAll(reportClean: true);
                CommandLog.Info(bad == 0 ? "all macros OK" : $"{bad} macro(s) have problems");
                yield break;
            }

            string name = MacroLibrary.ToKey(ctx.Inv.Args);
            var problems = MacroCheck.Problems(name);
            if (problems.Count == 0)
                CommandLog.Info($"{name}: OK");
            foreach (var problem in problems)
                CommandLog.Warning($"{name}, {problem}");
        }
    }

    [Command("record", Args = "<macro name>",
        Help = "Start recording every command you type or say; end saves them as a macro. Never overwrites an existing macro")]
    public class RecordCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => inv.HasArgs ? null : "say what to call it, e.g. record heart tour";

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (MacroRecorder.IsRecording)
            {
                CommandLog.Warning($"already recording \"{MacroRecorder.RecordingName}\" -- end it first");
                yield break;
            }
            MacroRecorder.Start(ctx.Inv.Args);
            CommandLog.Info($"recording \"{MacroRecorder.RecordingName}\" -- type or say commands, then end");
        }
    }

    [Command("end", Aliases = new[] { "stop recording", "end recording" }, Help = "Stop recording and save the macro")]
    public class EndCmd : AnatomyCommand
    {
        public override IEnumerator Execute(CommandContext ctx)
        {
            if (!MacroRecorder.IsRecording)
            {
                CommandLog.Warning("not recording -- start with record <name>");
                yield break;
            }
            string name = MacroRecorder.RecordingName;
            string path = MacroRecorder.Finish(out int count, out string error);
            if (path == null)
                CommandLog.Error($"could not save \"{name}\": {error}");
            else
                CommandLog.Info($"saved {count} command(s) to StreamingAssets/macros/{Path.GetFileName(path)} -- run it with: {MacroLibrary.ToKey(Path.GetFileNameWithoutExtension(path))}");
        }
    }

    [Command("help", Aliases = new[] { "?" }, Args = "[verb]", Help = "List commands, or describe one")]
    public class HelpCmd : AnatomyCommand
    {
        public override IEnumerator Execute(CommandContext ctx)
        {
            if (!ctx.Inv.HasArgs)
            {
                foreach (var command in CommandRegistry.All)
                    CommandLog.Info($"{command.Usage,-34} {command.Info.Help}");
                CommandLog.Info("Separate commands with ; or new lines. wait N pauses, & runs in parallel, queue ... waits for what's running.");
                yield break;
            }

            var found = CommandRegistry.Find(ctx.Inv.Args);
            if (found == null)
            {
                CommandLog.Warning($"no command \"{ctx.Inv.Args}\"");
                yield break;
            }
            CommandLog.Info(found.Usage);
            CommandLog.Info("  " + found.Info.Help);
            if (found.Info.Aliases.Length > 0)
                CommandLog.Info("  also: " + string.Join(", ", found.Info.Aliases));
            if (found.Info.Phrases.Length > 0)
                CommandLog.Info("  phrases: " + string.Join(", ", found.Info.Phrases));
        }
    }

    [Command("say", Args = "\"<spoken text>\"",
        Help = "Run text through the voice interpreter without the microphone, to test how a phrase is understood")]
    public class SayCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => inv.HasArgs ? null : "say what, e.g. say \"show me the heart\"";

        public override IEnumerator Execute(CommandContext ctx)
        {
            string spoken = ctx.Inv.Quoted.FirstOrDefault() ?? ctx.Inv.Args;
            string commandText = VoiceInterpreter.Interpret(spoken);
            if (commandText == null)
                yield break;

            var parsed = CommandParser.Parse(commandText);
            foreach (var error in parsed.Errors)
                CommandLog.Error(error.ToString());
            yield return ctx.Runner.RunList(parsed.Commands, ctx.Depth + 1, ctx.Source);
        }
    }
}
