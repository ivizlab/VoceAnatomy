using System.Collections;
using System.Linq;

namespace Assets.Scripts.Scripting
{
    [Command("slice", Aliases = new[] { "cross section", "cross" },
        Phrases = new[] { "slow slice => slice slow", "remove slice => slice off", "hide slice => slice off", "reset slice => slice off" },
        Args = "[on | off | at <percent> | <percent> | back | slow]",
        Help = "Front-to-back cross-section, 0% = front. slice at N jumps there, slice N steps N deeper, slice back steps 10% toward the front, slice slow sweeps through and back")]
    public class SliceCmd : AnatomyCommand
    {
        private static readonly string[] Allowed = { "on", "off", "remove", "reset", "at", "to", "back", "slow", "percent", "deeper", "by" };

        public override string Validate(CommandInvocation inv)
        {
            var unknown = inv.Words.Where(w => !Allowed.Contains(w) && !float.TryParse(w, out _)).ToList();
            if (unknown.Count > 0)
                return $"didn't understand \"{string.Join(" ", unknown)}\"";
            if (inv.HasAny("at", "to") && !inv.TryGetNumber(out _))
                return "slice at needs a percentage, e.g. slice at 40";
            return null;
        }

        public override IEnumerator Execute(CommandContext ctx)
        {
            var slices = CrossSections.Instance;
            var inv = ctx.Inv;
            bool hasNumber = inv.TryGetNumber(out float percent);

            if (inv.HasAny("off", "remove", "reset"))
                slices.VoiceRemoveSlice();
            else if (inv.Has("slow"))
            {
                slices.VoiceSlowSlice();
                yield return WaitWhile(() => slices.IsAutoSlicing);
            }
            else if (inv.Has("back"))
                slices.VoiceStepBack();
            else if (hasNumber && inv.HasAny("at", "to"))
                slices.VoiceJumpTo(percent);
            else if (hasNumber)
                slices.VoiceStepDeeper(percent);
            else
                slices.VoiceSliceOn();
        }
    }

    [Command("reset", Phrases = new[] { "start over => reset", "whole body => reset" }, Args = "[view]",
        Help = "reset: restore the whole body and camera; reset view: camera only")]
    public class ResetCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) =>
            !inv.HasArgs || inv.HasAny("view", "camera") ? null : "say reset, or reset view";

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (ctx.Inv.HasAny("view", "camera"))
            {
                CameraController.instance.ResetCamera();
            }
            else
            {
                CaptionPanel.ClearIfShown();
                ActionControl.Instance.ResetAll();
            }
            yield break;
        }
    }

    [Command("clear", Help = "Remove the slice and face front, keeping zoom and what's visible")]
    public class ClearCmd : AnatomyCommand
    {
        public override IEnumerator Execute(CommandContext ctx)
        {
            CrossSections.Instance.VoiceRemoveSlice();
            CameraController.instance.ResetOrientation();
            yield break;
        }
    }

    [Command("gesture", Aliases = new[] { "gestures" }, Args = "off | leap | video", Help = "Switch hand-tracking input")]
    public class GestureCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) =>
            inv.HasAny("off", "disable", "leap", "video", "camera") ? null : "say gesture off, leap or video";

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (ctx.Inv.Has("leap"))
                HandInputCoordinator.SetMode(HandInputMode.Leap);
            else if (ctx.Inv.HasAny("video", "camera"))
                HandInputCoordinator.SetMode(HandInputMode.Video);
            else
                HandInputCoordinator.SetMode(HandInputMode.Off);
            yield break;
        }
    }
}
