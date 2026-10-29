using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    [Command("focus", Aliases = new[] { "focus in", "frame" },
        Help = "Frame what was last shown; with nothing shown yet, frame everything visible")]
    public class FocusCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => inv.HasArgs ? "takes no arguments (it frames what was last shown)" : null;

        public override IEnumerator Execute(CommandContext ctx)
        {
            FrameLastShown();
            yield return WaitForCamera();
        }

        internal static void FrameLastShown()
        {
            SelectedObjectsManagement.Instance.DeselectAllObjects();
            var targets = CommandSelection.LastShown;
            if (targets != null)
            {
                foreach (var obj in targets.Where(o => o != null))
                    SelectedObjectsManagement.Instance.SelectObject(obj);
            }
            CameraController.instance.CenterView(true);
        }
    }

    [Command("zoom", Args = "in | out [percent]",
        Help = "zoom in N / zoom out N change distance by N percent; plain zoom in frames what was last shown, plain zoom out frames the whole visible body")]
    public class ZoomCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => inv.HasAny("in", "out") ? null : "say zoom in or zoom out";

        public override IEnumerator Execute(CommandContext ctx)
        {
            bool zoomIn = ctx.Inv.Has("in");
            if (ctx.Inv.TryGetNumber(out float percent))
            {
                CameraController.instance.ZoomByPercent(zoomIn ? -percent : percent);
            }
            else if (zoomIn)
            {
                FocusCmd.FrameLastShown();
            }
            else
            {
                SelectedObjectsManagement.Instance.DeselectAllObjects();
                CameraController.instance.CenterView(true);
            }
            yield return WaitForCamera();
        }
    }

    [Command("rotate", Aliases = new[] { "turn", "spin" },
        Phrases = new[] { "slow rotate => rotate slow", "slow turn => rotate slow", "slow spin => rotate slow" },
        Args = "[slow | <degrees>]",
        Help = "rotate: one full turn (2 s); rotate slow: one full turn (4 s); rotate N: turn N degrees and stop")]
    public class RotateCmd : AnatomyCommand
    {
        private static readonly string[] Allowed = { "slow", "by", "degrees", "degree" };

        public override string Validate(CommandInvocation inv)
        {
            var unknown = inv.Words.Where(w => !Allowed.Contains(w) && !float.TryParse(w, out _)).ToList();
            return unknown.Count > 0 ? $"didn't understand \"{string.Join(" ", unknown)}\"" : null;
        }

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (ctx.Inv.TryGetNumber(out float degrees))
                CameraController.instance.RotateBy(degrees);
            else if (ctx.Inv.Has("slow"))
                CameraController.instance.StartAutoRotate(4f);
            else
                CameraController.instance.StartAutoRotate(2f);
            yield return WaitForCamera();
        }
    }

    [Command("pan", Args = "up | down | left | right [percent]", Help = "Move the view; default 15 percent of the view distance")]
    public class PanCmd : AnatomyCommand
    {
        private const float DefaultPercent = 15f;

        public override string Validate(CommandInvocation inv) =>
            inv.HasAny("up", "down", "left", "right") ? null : "say pan up, down, left or right";

        public override IEnumerator Execute(CommandContext ctx)
        {
            float percent = ctx.Inv.TryGetNumber(out float n) ? n : DefaultPercent;
            if (ctx.Inv.Has("up")) CameraController.instance.PanByPercent(percent);
            else if (ctx.Inv.Has("down")) CameraController.instance.PanByPercent(-percent);
            else if (ctx.Inv.Has("right")) CameraController.instance.PanHorizontalByPercent(percent);
            else CameraController.instance.PanHorizontalByPercent(-percent);
            yield break;
        }
    }

    [Command("view", Phrases = new[] { "front => view front", "face front => view front" },
        Args = "front | back | left | right | top | bottom",
        Help = "Turn to a standard view, keeping zoom and centre")]
    public class ViewCmd : AnatomyCommand
    {
        // (pitch, yaw) for each named view; front is the app's default orientation.
        private static readonly (string name, float pitch, float yaw)[] Views =
        {
            ("front", 0, 0), ("back", 0, 180), ("left", 0, 90), ("right", 0, -90), ("top", 90, 0), ("bottom", -90, 0),
        };

        public override string Validate(CommandInvocation inv) =>
            Views.Any(v => inv.Has(v.name)) ? null : "say view front, back, left, right, top or bottom";

        public override IEnumerator Execute(CommandContext ctx)
        {
            var view = Views.First(v => ctx.Inv.Has(v.name));
            CameraController.instance.SetOrientation(view.pitch, view.yaw);
            yield break;
        }
    }
}
