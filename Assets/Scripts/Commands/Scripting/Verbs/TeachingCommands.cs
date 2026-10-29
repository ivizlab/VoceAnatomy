using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>Shared argument handling for caption and title: [seconds] "text", or clear.</summary>
    internal static class NarrationArgs
    {
        private static readonly Regex LeadingSeconds = new Regex(@"^\s*(\d+(?:\.\d+)?)\s+(.+)$", RegexOptions.Singleline);

        public static bool IsClear(CommandInvocation inv) =>
            inv.Quoted.Count == 0 && inv.Words.Count == 1 && (inv.Has("clear") || inv.Has("off") || inv.Has("hide"));

        /// <summary>Text to show (quoted text if any, else the raw argument) and an optional leading
        /// duration in seconds: caption 5 "The aorta carries blood to the body".</summary>
        public static string Text(CommandInvocation inv, out float? seconds)
        {
            seconds = null;
            string args = inv.Args.Trim();
            var match = LeadingSeconds.Match(args);
            if (match.Success)
            {
                seconds = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                args = match.Groups[2].Value.Trim();
            }
            if (args.StartsWith("\"") && inv.Quoted.Count > 0)
                return inv.Quoted[0];
            return args;
        }

        public static string Validate(CommandInvocation inv) =>
            IsClear(inv) || Text(inv, out _).Length > 0 ? null : "needs text, e.g. caption \"The heart has four chambers.\"";
    }

    [Command("caption", Args = "[seconds] \"text\" | clear",
        Help = "Narration in the lower third. Stays until the next caption or caption clear; with seconds, shows that long and the macro waits. *bold* and _italic_ work")]
    public class CaptionCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => NarrationArgs.Validate(inv);

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (NarrationArgs.IsClear(ctx.Inv))
            {
                CaptionPanel.Instance.ClearCaption();
                yield break;
            }
            CaptionPanel.Instance.ShowCaption(NarrationArgs.Text(ctx.Inv, out float? seconds));
            if (seconds.HasValue)
            {
                yield return new WaitForSeconds(seconds.Value);
                CaptionPanel.Instance.ClearCaption();
            }
        }
    }

    [Command("title", Args = "[seconds] \"text\" | clear", Help = "Large heading at the top of the screen, for section breaks. title clear removes it")]
    public class TitleCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => NarrationArgs.Validate(inv);

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (NarrationArgs.IsClear(ctx.Inv))
            {
                CaptionPanel.Instance.ClearTitle();
                yield break;
            }
            CaptionPanel.Instance.ShowTitle(NarrationArgs.Text(ctx.Inv, out float? seconds));
            if (seconds.HasValue)
            {
                yield return new WaitForSeconds(seconds.Value);
                CaptionPanel.Instance.ClearTitle();
            }
        }
    }

    [Command("describe", Args = "[part]",
        Help = "Put the opening of the part's description (the selection's, if none is named) in the caption. The full text is in the description panel")]
    public class DescribeCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => PartTargets.Validate(PartTargets.Items(inv), allowEmpty: true);

        public override IEnumerator Execute(CommandContext ctx)
        {
            List<PartEntry> entries;
            if (ctx.Inv.HasArgs)
            {
                entries = PartTargets.FoundEntries(PartTargets.Resolve(PartTargets.Items(ctx.Inv)));
                if (entries.Count == 0)
                    yield break;
                PartActions.Show(entries);
                PartActions.Select(entries, additive: false);
            }
            else
            {
                entries = PartActions.NamedSelection();
            }

            var entry = entries.FirstOrDefault(e => e.Node != null && e.Node.GetComponent<NameAndDescription>() != null);
            if (entry == null)
            {
                CommandLog.Warning("nothing selected to describe -- name a part, e.g. describe aorta");
                yield break;
            }

            string lead = DescriptionText.Lead(entry.Node.GetComponent<NameAndDescription>());
            if (lead == null)
            {
                CommandLog.Warning($"no description for {entry.Name}");
                yield break;
            }
            CaptionPanel.Instance.ShowCaption($"<b>{entry.Name}</b>\n<size=85%>{lead}</size>");
        }
    }

    /// <summary>The opening of a part's description, short enough for a caption. Descriptions are long
    /// articles (median ~900 characters, some far longer), so this keeps the first sentences of the
    /// introduction and drops the heading line and leftover empty parentheses.</summary>
    public static class DescriptionText
    {
        private const int Sentences = 2;
        private const int MaxChars = 360;

        private static readonly Regex JunkParentheses = new Regex(@"\s*\((?=[\s,;])[^()]*\)");
        private static readonly Regex SentenceEnd = new Regex(@"(?<=[.!?])\s+(?=[A-Z])");

        public static string Lead(NameAndDescription part)
        {
            if (ReadLocalDefinitions.Instance == null)
                return null;
            bool check = false;
            // Same key the description panel uses (NameAndDescription.SetDescription).
            string full = ReadLocalDefinitions.Instance.GetDescription(part.originalName.RemoveSuffix().ToLower(), ref check);
            return string.IsNullOrWhiteSpace(full) ? null : Lead(full);
        }

        public static string Lead(string full)
        {
            string text = full.Replace("\r", "");
            int section = text.IndexOf("\n== ");
            if (section > 0)
                text = text.Substring(0, section);

            var lines = text.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            if (lines.Count > 0 && lines[0] == lines[0].ToUpperInvariant())
                lines.RemoveAt(0); // "HEART" heading
            text = JunkParentheses.Replace(string.Join(" ", lines), "");
            text = Regex.Replace(text, @"\s{2,}", " ").Replace(" ,", ",").Trim();

            text = string.Join(" ", SentenceEnd.Split(text).Take(Sentences));
            if (text.Length > MaxChars)
            {
                int cut = text.LastIndexOf(' ', MaxChars);
                text = text.Substring(0, cut > 0 ? cut : MaxChars) + "…";
            }
            return text.Length > 0 ? text : null;
        }
    }
}
