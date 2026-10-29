using Assets.Scripts.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// What "focus" frames: everything the most recent show named. Interim -- the part-resolver phase
    /// replaces this with the app's real selection (SelectedObjectsManagement), so a mouse click, a voice
    /// command and a macro all share one "current part".
    /// </summary>
    public static class CommandSelection
    {
        public static List<GameObject> LastShown;
    }

    /// <summary>The body systems (layers) a show/hide can name, and the words for each.</summary>
    internal static class BodySystems
    {
        private static readonly (string[] names, Func<List<TangibleBodyPart>> parts)[] Systems =
        {
            (new[] { "muscles", "muscle", "muscular" }, () => GlobalVariables.Instance.muscles),
            (new[] { "skeleton", "bones", "bone", "skeletal" }, () => GlobalVariables.Instance.bones),
            (new[] { "nerves", "nerve", "nervous" }, () => GlobalVariables.Instance.nerves),
            (new[] { "organs", "organ", "viscera" }, () => GlobalVariables.Instance.viscera),
            (new[] { "blood vessels", "vessels", "vessel", "arteries", "artery", "veins", "vein" },
                () => GlobalVariables.Instance.arteries.Concat(GlobalVariables.Instance.veins).ToList()),
            (new[] { "lymph", "lymphatic", "lymphatics" }, () => GlobalVariables.Instance.lymphs),
            (new[] { "joints", "joint" }, () => GlobalVariables.Instance.joints),
        };

        public static IEnumerable<string> Names => Systems.SelectMany(s => s.names);

        /// <summary>Exact match only, so "vagus nerve" is a part, not the nerves layer. Tolerates a leading
        /// "the" and a trailing "system" ("the muscular system").</summary>
        public static bool TryGet(string item, out List<TangibleBodyPart> parts)
        {
            string key = item.Trim().ToLowerInvariant();
            if (key.StartsWith("the "))
                key = key.Substring(4);
            if (key.EndsWith(" system"))
                key = key.Substring(0, key.Length - " system".Length);

            foreach (var system in Systems)
            {
                if (system.names.Contains(key))
                {
                    parts = system.parts();
                    return true;
                }
            }
            parts = null;
            return false;
        }
    }

    [Command("show", Aliases = new[] { "add" }, Args = "<system | part>[, ...]",
        Help = "Make a body system or part visible; focus then frames it. e.g. show muscles, show heart and lungs")]
    public class ShowCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => inv.Items.Count == 0 ? "say what to show" : null;

        public override IEnumerator Execute(CommandContext ctx)
        {
            var named = new List<GameObject>();
            foreach (var item in ctx.Inv.Items)
            {
                if (BodySystems.TryGet(item, out var parts))
                    named.AddRange(ShowParts(parts));
                else
                    named.AddRange(ShowStructure(item));
            }
            Lexicon.Instance.UpdateTreeViewCheckboxes();

            if (named.Count > 0)
                CommandSelection.LastShown = named;
            yield break;
        }

        // Returns everything in the layer, not just what this call made visible: "focus" needs the full
        // set, while the undo command deliberately records only what actually changed.
        private static IEnumerable<GameObject> ShowParts(List<TangibleBodyPart> parts)
        {
            var shown = new List<GameObject>();
            foreach (var part in parts)
            {
                if (part.gameObject.activeInHierarchy)
                    continue;
                part.transform.SetActiveParentsRecursively(true);
                shown.Add(part.gameObject);
            }
            ActionControl.Instance.AddCommand(new ShowCommand(shown), true);
            return parts.Select(p => p.gameObject);
        }

        private static IEnumerable<GameObject> ShowStructure(string name)
        {
            Transform found = GlobalVariables.Instance.globalParent.transform.RecursiveFindChild(name);
            if (found == null)
            {
                CommandLog.Warning($"no structure named \"{name}\"");
                return Enumerable.Empty<GameObject>();
            }

            var shown = new List<GameObject>();
            found.SetActiveParentsRecursively(true, shown);
            found.SetActiveRecursively(true, shown);
            ActionControl.Instance.AddCommand(new ShowCommand(shown), false);
            SelectedObjectsManagement.Instance.GetActiveObjects();

            // Same reasoning as ShowParts: focus needs the whole structure, including parts that were
            // already visible. Falls back to the node itself so this is never empty.
            var structure = found.GetComponentsInChildren<TangibleBodyPart>(true).Select(t => t.gameObject).ToList();
            if (structure.Count == 0)
                structure.Add(found.gameObject);
            return structure;
        }
    }

    [Command("hide", Aliases = new[] { "remove" }, Args = "<system | part>[, ...]",
        Help = "Hide a body system or part. e.g. hide skeleton, hide ribs")]
    public class HideCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => inv.Items.Count == 0 ? "say what to hide" : null;

        public override IEnumerator Execute(CommandContext ctx)
        {
            foreach (var item in ctx.Inv.Items)
            {
                List<GameObject> targets;
                if (BodySystems.TryGet(item, out var parts))
                {
                    targets = parts.Select(p => p.gameObject).ToList();
                }
                else
                {
                    Transform found = GlobalVariables.Instance.globalParent.transform.RecursiveFindChild(item);
                    if (found == null)
                    {
                        CommandLog.Warning($"no structure named \"{item}\"");
                        continue;
                    }
                    targets = found.GetComponentsInChildren<TangibleBodyPart>(true).Select(t => t.gameObject).ToList();
                }

                var hidden = targets.Where(go => go.activeInHierarchy).ToList();
                ActionControl.Instance.AddCommand(new DeleteCommand(hidden), true);
            }
            yield break;
        }
    }
}
