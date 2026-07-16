using TMPro;
using UnityEngine;

/// <summary>
/// Small branding title shown top-center of the screen. Builds its own UI
/// element at startup instead of relying on prefab-authored UI, so no manual
/// Canvas/RectTransform setup is needed -- attach this component to any
/// GameObject that exists in MainScene (e.g. the same one VoiceCommands is on).
/// </summary>
public class BrandingTitle : MonoBehaviour
{
    [SerializeField] private string text = "SFU Research (via V-Anatomy)";
    [SerializeField] private int fontSize = 20;
    [SerializeField] private float topMargin = 10f;
    [SerializeField, Range(0f, 1f)] private float opacity = 0.85f;

    private void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[BrandingTitle] No Canvas found in scene -- title not shown.");
            return;
        }

        GameObject go = new GameObject("BrandingTitle", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -topMargin);
        rt.sizeDelta = new Vector2(600f, 40f);

        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 1f, 1f, opacity);
        label.raycastTarget = false;
    }
}
