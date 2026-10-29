using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps the skin layer and the external genitalia from rendering while the app starts up.
/// The full model is active in the scene, and <see cref="Layers"/> only hides the skin once the
/// lexicon has been built (several seconds after Play), so without this the first seconds show
/// a fully nude figure.
///
/// Hides those renderers with <see cref="Renderer.forceRenderingOff"/> right after the scene
/// loads (before the first frame is drawn), then releases them once the app has deactivated the
/// skin itself, so normal behavior afterwards (e.g. the skin slider) is unchanged. Also used by
/// the editor-only EditModePreviewFilter to hide the same parts before Play.
/// </summary>
public static class StartupModestyFilter
{
    // Hierarchy groups hidden in full, in addition to everything in the skin layer lists.
    private static readonly string[] HiddenGroups = { "Male external genitalia.g" };

    // Never hold the parts hidden longer than this, even if the skin is never switched off.
    private const float MaxHoldSeconds = 30f;

    /// <summary>Renderers of the skin layer, and of the hidden groups, under the body root.
    /// Empty if the scene isn't the main scene.</summary>
    public static void FindRenderers(out List<Renderer> skin, out List<Renderer> groups)
    {
        skin = new List<Renderer>();
        groups = new List<Renderer>();

        var globals = Object.FindObjectOfType<GlobalVariables>(true);
        var layers = Object.FindObjectOfType<Layers>(true);
        if (globals == null || globals.globalParent == null)
            return;

        // Same name lists Layers.ReadLayer uses: first line is the layer's own name, the rest
        // are body part names (matched with and without their ".r"/".l" suffix).
        var skinNames = new HashSet<string>();
        if (layers != null && layers.skinLayers != null)
        {
            foreach (var layer in layers.skinLayers)
            {
                if (layer == null)
                    continue;
                string[] lines = layer.text.Split('\n');
                for (int i = 1; i < lines.Length; i++)
                {
                    string name = lines[i].Replace("\r", "").Trim();
                    if (name.Length == 0)
                        continue;
                    skinNames.Add(name);
                    skinNames.Add(name.RemoveSuffix());
                }
            }
        }

        var groupRoots = new List<Transform>();
        foreach (var t in globals.globalParent.GetComponentsInChildren<Transform>(true))
        {
            if (System.Array.IndexOf(HiddenGroups, t.name) >= 0)
                groupRoots.Add(t);
        }

        foreach (var renderer in globals.globalParent.GetComponentsInChildren<Renderer>(true))
        {
            string name = renderer.gameObject.name;
            if (skinNames.Contains(name) || skinNames.Contains(name.RemoveSuffix()))
            {
                skin.Add(renderer);
                continue;
            }
            foreach (var root in groupRoots)
            {
                if (renderer.transform.IsChildOf(root))
                {
                    groups.Add(renderer);
                    break;
                }
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void HideDuringStartup()
    {
        FindRenderers(out var skin, out var groups);
        if (skin.Count == 0 && groups.Count == 0)
            return;

        foreach (var r in skin) r.forceRenderingOff = true;
        foreach (var r in groups) r.forceRenderingOff = true;

        // Runs on GlobalVariables (always active in the main scene) rather than a new component.
        Object.FindObjectOfType<GlobalVariables>().StartCoroutine(ReleaseWhenSkinHidden(skin, groups));
    }

    private static IEnumerator ReleaseWhenSkinHidden(List<Renderer> skin, List<Renderer> groups)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < MaxHoldSeconds && AnySkinActive(skin))
            yield return null;

        // One more frame so anything deactivated alongside the skin settles before release.
        yield return null;

        foreach (var r in skin) if (r != null) r.forceRenderingOff = false;
        foreach (var r in groups) if (r != null) r.forceRenderingOff = false;
        float held = Time.realtimeSinceStartup - start;
        if (held >= MaxHoldSeconds)
            Debug.LogWarning($"[StartupModestyFilter] Skin was still active after {MaxHoldSeconds:F0}s -- released anyway.");
        else
            Debug.Log($"[StartupModestyFilter] Skin hidden by the app after {held:F1}s -- released.");
    }

    // "Still showing" until at least 95% of the skin parts are inactive -- tolerates a few
    // name-list matches that aren't actually toggled by the skin slider.
    private static bool AnySkinActive(List<Renderer> skin)
    {
        int active = 0;
        foreach (var r in skin)
        {
            if (r != null && r.gameObject.activeInHierarchy)
                active++;
        }
        return active > skin.Count * 0.05f;
    }
}
