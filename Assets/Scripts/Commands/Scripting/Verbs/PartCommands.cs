using Assets.Scripts.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>The body systems (layers) a command can name, and the words for each.</summary>
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

    /// <summary>What one item of a part-naming command ("heart", "muscles", "left kidney") refers to.</summary>
    internal class PartTarget
    {
        public string Item;
        /// <summary>Set when the item names a whole body system.</summary>
        public List<TangibleBodyPart> System;
        /// <summary>Set when the item names parts; check <see cref="PartLookup.Found"/>.</summary>
        public PartLookup Lookup;

        public bool IsSystem => System != null;
        public bool Found => IsSystem || Lookup.Found;
    }

    internal static class PartTargets
    {
        /// <summary>Splits a command's argument into part names on commas and "and" -- unless the whole
        /// argument is itself a name, since some contain "and" ("inferior occipital gyrus and sulcus").</summary>
        public static List<string> Items(CommandInvocation inv, string args = null)
        {
            args = args ?? inv.Args;
            var items = CommandParser.SplitItems(args);
            if (items.Count > 1 && PartResolver.IsReady)
            {
                string whole = args.Replace("\"", "").Trim();
                var lookup = PartResolver.Resolve(whole);
                if (lookup.Kind == PartMatchKind.Exact || lookup.Kind == PartMatchKind.Alias)
                    return new List<string> { whole };
            }
            return items;
        }

        public static List<PartTarget> Resolve(IEnumerable<string> items) =>
            items.Select(item => BodySystems.TryGet(item, out var parts)
                ? new PartTarget { Item = item, System = parts }
                : new PartTarget { Item = item, Lookup = PartResolver.Resolve(item) }).ToList();

        /// <summary>Parse-time check, so a macro with a misspelled or ambiguous part is reported when it
        /// loads. Skipped outside Play mode, where there's no model to check against.</summary>
        public static string Validate(IEnumerable<string> items, bool allowEmpty)
        {
            var list = items.ToList();
            if (list.Count == 0)
                return allowEmpty ? null : "say which part";
            if (!PartResolver.IsReady)
                return null;
            return Resolve(list).Where(t => !t.Found).Select(t => t.Lookup.Problem).FirstOrDefault();
        }

        /// <summary>Logs what each item resolved to ("hart -> Heart (17 parts)") and returns the entries
        /// that were found. Items that weren't are reported and skipped.</summary>
        public static List<PartEntry> FoundEntries(List<PartTarget> targets)
        {
            var entries = new List<PartEntry>();
            foreach (var target in targets.Where(t => !t.IsSystem))
            {
                if (!target.Lookup.Found)
                {
                    CommandLog.Warning(target.Lookup.Problem);
                    continue;
                }
                int count = PartActions.Leaves(target.Lookup.Entries).Count;
                CommandLog.Info($"  {target.Item} -> {target.Lookup.MatchedName} ({count} part{(count == 1 ? "" : "s")})");
                entries.AddRange(target.Lookup.Entries);
            }
            return entries.Distinct().ToList();
        }

        public static List<GameObject> SystemLeaves(List<PartTarget> targets) =>
            targets.Where(t => t.IsSystem).SelectMany(t => t.System).Select(p => p.gameObject).Distinct().ToList();
    }

    /// <summary>
    /// Visibility and selection changes for named parts, built on the app's own systems: the same
    /// SelectedObjectsManagement selection a mouse click makes, and ShowCommand/DeleteCommand/SelectCommand
    /// so the undo button still works. Groups select their children the way a click in the lexicon does.
    /// </summary>
    internal static class PartActions
    {
        private static SelectedObjectsManagement Selection => SelectedObjectsManagement.Instance;

        /// <summary>The meshes a set of entries stands for: the part and every part under it (a label
        /// stands for the part it labels).</summary>
        public static List<GameObject> Leaves(IEnumerable<PartEntry> entries) =>
            entries.SelectMany(e => e.IsLabel
                    ? new[] { e.Node.GetComponent<Label>().parent.gameObject }
                    : e.Node.GetComponentsInChildren<TangibleBodyPart>(true).Where(t => !t.CompareTag("Insertions")).Select(t => t.gameObject))
                .Distinct().ToList();

        private static List<PartEntry> lastNamed;
        private static List<GameObject> lastNamedSelection;

        /// <summary>What the selection "is" by name: the entries a command last named, as long as the
        /// selection hasn't changed since (a named group selects its children, so "show heart; describe"
        /// must describe the heart, not its first chamber); otherwise the selected objects themselves.</summary>
        public static List<PartEntry> NamedSelection()
        {
            if (lastNamed != null && lastNamedSelection.SequenceEqual(Selection.selectedObjects))
                return lastNamed;
            return Selection.selectedObjects
                .Where(o => o != null && o.GetComponent<NameAndDescription>() != null)
                .Select(o => new PartEntry { Name = o.name, Node = o, IsLabel = o.IsLabel() })
                .ToList();
        }

        public static List<GameObject> SelectedLeaves() => Selection.selectedObjects
            .Select(o => o.IsLabel() ? o.GetComponent<Label>().parent.gameObject : o)
            .Where(o => o.IsBodyPart()).Distinct().ToList();

        public static void Show(IEnumerable<PartEntry> entries)
        {
            var shown = new List<GameObject>();
            foreach (var entry in entries)
            {
                entry.Node.transform.SetActiveParentsRecursively(true, shown);
                if (entry.IsLabel)
                {
                    var line = entry.Node.GetComponent<Label>().line;
                    if (line != null)
                        line.gameObject.SetActive(true);
                }
                else
                {
                    entry.Node.transform.SetActiveRecursively(true, shown);
                }
            }
            ActionControl.Instance.AddCommand(new ShowCommand(shown), false);
        }

        public static void ShowSystem(List<TangibleBodyPart> parts)
        {
            var shown = new List<GameObject>();
            foreach (var part in parts.Where(p => !p.gameObject.activeInHierarchy))
            {
                part.transform.SetActiveParentsRecursively(true);
                shown.Add(part.gameObject);
            }
            ActionControl.Instance.AddCommand(new ShowCommand(shown), true);
        }

        /// <summary>Makes the entries the selection (or adds them to it). Mirrors a click on a name in the
        /// description panel: parts select themselves, groups select everything under them, a label selects
        /// its part and itself.</summary>
        public static void Select(List<PartEntry> entries, bool additive)
        {
            if (!additive)
                Selection.DeselectAllObjects();

            foreach (var entry in entries)
            {
                if (entry.IsLabel)
                {
                    Selection.SelectObject(entry.Node.GetComponent<Label>().parent.gameObject);
                    Selection.SelectObject(entry.Node);
                }
                else if (entry.Node.IsBodyPart())
                {
                    Selection.SelectObject(entry.Node);
                }
                else
                {
                    Selection.SelectAllChildren(entry.Node.transform);
                }
            }

            Selection.selectedObjects = Selection.selectedObjects.Distinct().ToList();
            ActionControl.Instance.AddCommand(new SelectCommand(Selection.selectedObjects), false);
            AfterChange();

            lastNamed = additive && lastNamed != null ? lastNamed.Concat(entries).Distinct().ToList() : entries;
            lastNamedSelection = new List<GameObject>(Selection.selectedObjects);

            // One named thing: fill the description panel and hierarchy bar, as clicking it would.
            var nodes = entries.Select(e => e.Node).Distinct().ToList();
            if (nodes.Count == 1)
                Selection.ShowBodyPartInfo(nodes[0]);
        }

        public static void Hide(IEnumerable<GameObject> objects)
        {
            var hidden = objects.Where(o => o != null && o.activeInHierarchy).Distinct().ToList();
            ActionControl.Instance.AddCommand(new DeleteCommand(hidden), true);
            AfterChange();
        }

        /// <summary>Hides every visible part not in <paramref name="keep"/>. Unlike the Space-key isolate
        /// this never toggles back, so a macro gets the same result however often it runs.</summary>
        public static void Isolate(List<GameObject> keep)
        {
            var keepSet = new HashSet<GameObject>(keep);
            var toHide = GlobalVariables.Instance.allBodyParts
                .Select(p => p.gameObject)
                .Where(o => o.activeInHierarchy && !keepSet.Contains(o))
                .ToList();
            ActionControl.Instance.AddCommand(new DeleteCommand(toHide, Selection.selectedObjects, false), true);
            // Lets the Space key un-isolate afterwards, same as an isolate made with the mouse.
            Selection.lastIsolatedObjects = new List<GameObject>(Selection.selectedObjects);
            AfterChange();
        }

        private static void AfterChange()
        {
            Selection.GetActiveObjects();
            ActionControl.Instance.UpdateButtons();
            Lexicon.Instance.UpdateSelected();
            Lexicon.Instance.UpdateTreeViewCheckboxes();
        }
    }

    // ----------------------------------------------------------------------
    // Verbs. Rule: a command that names parts makes them the selection; a command that names nothing acts
    // on the selection. Naming a whole system changes visibility only and clears the selection, so focus
    // then frames everything visible.
    // ----------------------------------------------------------------------

    /// <summary>Shared body of show/select/also: show the named things and select the named parts.</summary>
    internal static class ShowAndSelect
    {
        public static void Run(CommandInvocation inv, bool additive)
        {
            var targets = PartTargets.Resolve(PartTargets.Items(inv));
            foreach (var target in targets.Where(t => t.IsSystem))
                PartActions.ShowSystem(target.System);

            var entries = PartTargets.FoundEntries(targets);
            if (entries.Count > 0)
            {
                PartActions.Show(entries);
                PartActions.Select(entries, additive);
            }
            else if (!additive && targets.Any(t => t.IsSystem))
            {
                SelectedObjectsManagement.Instance.DeselectAllObjects();
            }
            Lexicon.Instance.UpdateTreeViewCheckboxes();
        }
    }

    [Command("show", Aliases = new[] { "add", "select" }, Args = "<part | system>[, ...]",
        Help = "Make parts or a body system visible and select the parts; focus, rotate and label then act on them. e.g. show heart, select left kidney, show muscles")]
    public class ShowCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => PartTargets.Validate(PartTargets.Items(inv), allowEmpty: false);

        public override IEnumerator Execute(CommandContext ctx)
        {
            ShowAndSelect.Run(ctx.Inv, additive: false);
            yield break;
        }
    }

    [Command("also", Args = "<part>[, ...]", Help = "Show parts and add them to the current selection. e.g. show heart; also aorta")]
    public class AlsoCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => PartTargets.Validate(PartTargets.Items(inv), allowEmpty: false);

        public override IEnumerator Execute(CommandContext ctx)
        {
            ShowAndSelect.Run(ctx.Inv, additive: true);
            yield break;
        }
    }

    [Command("isolate", Args = "[part | system, ...]",
        Help = "Show only the named parts (or the current selection), hiding everything else. Space undoes it, as usual")]
    public class IsolateCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => PartTargets.Validate(PartTargets.Items(inv), allowEmpty: true);

        public override IEnumerator Execute(CommandContext ctx)
        {
            List<GameObject> keep;
            if (ctx.Inv.HasArgs)
            {
                ShowAndSelect.Run(ctx.Inv, additive: false);
                var targets = PartTargets.Resolve(PartTargets.Items(ctx.Inv));
                keep = PartTargets.SystemLeaves(targets).Concat(PartActions.SelectedLeaves()).ToList();
            }
            else
            {
                keep = PartActions.SelectedLeaves();
                if (keep.Count == 0)
                {
                    CommandLog.Warning("nothing selected to isolate -- name a part, e.g. isolate heart");
                    yield break;
                }
            }
            PartActions.Isolate(keep);
        }
    }

    [Command("hide", Aliases = new[] { "remove" }, Args = "[part | system, ...]",
        Help = "Hide parts or a body system; with nothing named, hide the selection. e.g. hide ribs, hide skeleton")]
    public class HideCmd : AnatomyCommand
    {
        public override string Validate(CommandInvocation inv) => PartTargets.Validate(PartTargets.Items(inv), allowEmpty: true);

        public override IEnumerator Execute(CommandContext ctx)
        {
            if (!ctx.Inv.HasArgs)
            {
                var selected = PartActions.SelectedLeaves();
                if (selected.Count == 0)
                    CommandLog.Warning("nothing selected to hide");
                else
                    PartActions.Hide(selected);
                yield break;
            }

            var targets = PartTargets.Resolve(PartTargets.Items(ctx.Inv));
            var entries = PartTargets.FoundEntries(targets);
            var objects = PartTargets.SystemLeaves(targets)
                .Concat(PartActions.Leaves(entries))
                .Concat(entries.Where(e => e.IsLabel).Select(e => e.Node));
            PartActions.Hide(objects);
        }
    }

    [Command("deselect", Phrases = new[] { "clear selection => deselect", "select none => deselect" },
        Help = "Clear the selection; focus then frames everything visible")]
    public class DeselectCmd : AnatomyCommand
    {
        public override IEnumerator Execute(CommandContext ctx)
        {
            SelectedObjectsManagement.Instance.DeselectAllObjects();
            yield break;
        }
    }

    [Command("label", Aliases = new[] { "labels" }, Args = "[on | off] [part, ...]",
        Help = "Show (or with off, hide) the model's text labels on the named parts or the selection. label off alone hides every label")]
    public class LabelCmd : AnatomyCommand
    {
        private static bool IsOff(CommandInvocation inv) => inv.Words.Count > 0 && (inv.Words[0] == "off" || inv.Words[0] == "hide");

        // The argument without its leading on/off word.
        private static string PartText(CommandInvocation inv)
        {
            string args = inv.Args.Trim();
            foreach (var word in new[] { "on", "off", "show", "hide" })
            {
                if (args.Equals(word, StringComparison.OrdinalIgnoreCase))
                    return "";
                if (args.StartsWith(word + " ", StringComparison.OrdinalIgnoreCase))
                    return args.Substring(word.Length).Trim();
            }
            return args;
        }

        public override string Validate(CommandInvocation inv) => PartTargets.Validate(PartTargets.Items(inv, PartText(inv)), allowEmpty: true);

        public override IEnumerator Execute(CommandContext ctx)
        {
            bool off = IsOff(ctx.Inv);
            string partText = PartText(ctx.Inv);

            if (partText.Length > 0)
            {
                var entries = PartTargets.FoundEntries(PartTargets.Resolve(PartTargets.Items(ctx.Inv, partText)));
                if (entries.Count == 0)
                    yield break;
                if (!off)
                {
                    PartActions.Show(entries);
                    PartActions.Select(entries, additive: false);
                }
            }

            var parts = PartActions.SelectedLeaves();
            if (parts.Count == 0)
            {
                if (off)
                    MeshManagement.Instance.HideAllLabels();
                else
                    CommandLog.Warning("nothing selected to label -- name a part, e.g. label femur");
                yield break;
            }

            int labelled = 0;
            foreach (var visibility in parts.Select(p => p.GetComponent<BodyPartVisibility>()).Where(v => v != null && v.HasLabels()))
            {
                if (off)
                    visibility.HideLabels(true);
                else
                    visibility.ShowLabels(true);
                labelled++;
            }
            if (!off && labelled == 0)
                CommandLog.Warning("the model has no text labels for " + (partText.Length > 0 ? partText : "the selection"));
        }
    }
}
