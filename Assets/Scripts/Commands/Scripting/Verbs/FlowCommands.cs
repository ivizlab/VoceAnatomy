using Assets.Scripts.Voice;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    [Command("wait", Args = "<seconds>", Help = "Pause a sequence, e.g. wait 3")]
    public class WaitCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) =>
            inv.TryGetNumber(out float seconds) && seconds >= 0 ? null : "needs a number of seconds, e.g. wait 3";

        public override IEnumerator Execute(CommandContext ctx)
        {
            ctx.Inv.TryGetNumber(out float seconds);
            yield return new WaitForSeconds(seconds);
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

    [Command("run", Args = "<macro name>", Help = "Run a macro file from StreamingAssets/macros; typing just the macro's name also works")]
    public class RunCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv)
        {
            if (string.IsNullOrWhiteSpace(inv.Args))
                return "say which macro (type macros for the list)";
            return MacroLibrary.Exists(inv.Args) ? null : $"no macro named \"{inv.Args}\" in StreamingAssets/macros";
        }

        public override IEnumerator Execute(CommandContext ctx)
        {
            string name = MacroLibrary.ToKey(ctx.Inv.Args);
            if (ctx.Depth >= CommandRunner.MaxMacroDepth)
            {
                CommandLog.Error($"macro \"{name}\" not run: macros nested more than {CommandRunner.MaxMacroDepth} deep (does it run itself?)");
                yield break;
            }
            if (!MacroLibrary.TryRead(name, out string text, out _))
            {
                CommandLog.Error($"macro \"{name}\" could not be read");
                yield break;
            }

            var parsed = CommandParser.Parse(text);
            foreach (var error in parsed.Errors)
                CommandLog.Error($"{name}, {error} -- skipped");

            yield return ctx.Runner.RunList(parsed.Commands, ctx.Depth + 1, CommandSource.Macro);
        }
    }

    [Command("macros", Help = "List the available macros")]
    public class MacrosCmd : AnatomyCommand
    {
        public override IEnumerator Execute(CommandContext ctx)
        {
            var names = MacroLibrary.Names.ToList();
            CommandLog.Info(names.Count == 0
                ? $"no macros yet -- add .macro files to {MacroLibrary.Folder}"
                : "macros: " + string.Join(", ", names));
            yield break;
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
