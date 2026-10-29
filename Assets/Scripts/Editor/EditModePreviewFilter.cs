using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor-only: hides the skin layer and the external genitalia while the project is open in
/// edit mode, so the Scene/Game view before pressing Play isn't a fully nude figure. The same
/// parts are found by <see cref="StartupModestyFilter"/>, which keeps them hidden during the
/// app's startup after Play is pressed.
///
/// Uses <see cref="Renderer.forceRenderingOff"/>, which is never written to the scene or prefab.
/// Entering Play reloads the scene, which clears it, so Play mode and builds are unaffected.
/// Toggle it from the "VoceAnatomy" menu; the choice is remembered per machine.
/// </summary>
[InitializeOnLoad]
public static class EditModePreviewFilter
{
    private const string PrefKey = "VoceAnatomy.EditModePreviewFilter";
    private const string MenuPath = "VoceAnatomy/Hide Skin and Genitalia in Edit Mode";

    private static readonly List<Renderer> hiddenRenderers = new List<Renderer>();

    static EditModePreviewFilter()
    {
        EditorApplication.delayCall += Apply;
        EditorSceneManager.sceneOpened += (scene, mode) => Apply();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        Enabled = !Enabled;
        Apply();
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // Not restored on ExitingEditMode on purpose: that would redraw the full figure for the
        // moment before Play starts. The scene reload on entering Play resets the flag anyway.
        if (state == PlayModeStateChange.EnteredEditMode)
            Apply();
    }

    private static void Apply()
    {
        Restore();
        if (!Enabled || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        StartupModestyFilter.FindRenderers(out var skin, out var groups);
        hiddenRenderers.AddRange(skin);
        hiddenRenderers.AddRange(groups);
        foreach (var renderer in hiddenRenderers)
            renderer.forceRenderingOff = true;

        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    private static void Restore()
    {
        foreach (var renderer in hiddenRenderers)
        {
            if (renderer != null)
                renderer.forceRenderingOff = false;
        }
        hiddenRenderers.Clear();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
