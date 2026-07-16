using UnityEngine;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Toggles a UI element on while push-to-talk voice input is listening.
    /// </summary>
    public class VoiceListeningIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject indicatorRoot;

        [Header("Placement -- small, unobtrusive corner badge")]
        [Tooltip("RectTransform anchor/pivot for indicatorRoot, e.g. (1,0) = bottom-right, (0,0) = bottom-left.")]
        [SerializeField] private Vector2 anchor = new Vector2(0f, 0f);
        [Tooltip("Distance in pixels from the anchored corner.")]
        [SerializeField] private Vector2 margin = new Vector2(20f, 20f);
        [SerializeField] private float scale = 0.3f;

        private void Awake()
        {
            if (indicatorRoot != null)
            {
                RectTransform rt = indicatorRoot.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = anchor;
                    rt.anchorMax = anchor;
                    rt.pivot = anchor;
                    rt.anchoredPosition = new Vector2(
                        anchor.x >= 0.5f ? -margin.x : margin.x,
                        anchor.y >= 0.5f ? -margin.y : margin.y);
                    rt.localScale = Vector3.one * scale;
                }
                indicatorRoot.SetActive(false);
            }
        }

        public void Show()
        {
            if (indicatorRoot != null)
                indicatorRoot.SetActive(true);
        }

        public void Hide()
        {
            if (indicatorRoot != null)
                indicatorRoot.SetActive(false);
        }
    }
}
