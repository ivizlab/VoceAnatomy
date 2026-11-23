using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// On-screen narration for lessons, separate from the 3D labels: a caption box in the lower third and
    /// a larger title at the top. Text may use TMP rich text, plus *word* for bold and _word_ for italic.
    /// Created on first use, with its own overlay canvas (below the command console), so it needs no scene
    /// setup. Never blocks clicks on the model.
    /// </summary>
    public class CaptionPanel : MonoBehaviour
    {
        private static CaptionPanel instance;

        public static CaptionPanel Instance
        {
            get
            {
                if (instance == null)
                {
                    var host = new GameObject("CaptionPanel");
                    if (CommandRunner.Instance != null)
                        host.transform.SetParent(CommandRunner.Instance.transform, false);
                    instance = host.AddComponent<CaptionPanel>();
                }
                return instance;
            }
        }

        /// <summary>Clears captions only if the panel was ever created -- for reset/stop, which shouldn't
        /// create UI just to hide it.</summary>
        public static void ClearIfShown()
        {
            if (instance != null)
                instance.ClearAll();
        }

        public static void HideWaitHintIfShown()
        {
            if (instance != null)
                instance.SetWaitHint(false);
        }

        private const float FadeSeconds = 0.25f;

        private Box caption;
        private Box title;
        private GameObject waitHint;

        private void Awake()
        {
            instance = this;
            BuildUI();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public void ShowCaption(string text) => Show(caption, text);
        public void ClearCaption() => Hide(caption);
        public void ShowTitle(string text) => Show(title, text);
        public void ClearTitle() => Hide(title);

        public void ClearAll()
        {
            Hide(caption);
            Hide(title);
            SetWaitHint(false);
        }

        /// <summary>The small "click or → to continue" cue shown while a lesson waits for the presenter.</summary>
        public void SetWaitHint(bool visible) => waitHint.SetActive(visible);

        // *bold* and _italic_ shorthand, so experts don't need to write TMP tags for simple emphasis.
        private static string Format(string text)
        {
            text = Regex.Replace(text, @"\*([^*\n]+)\*", "<b>$1</b>");
            text = Regex.Replace(text, @"(?<![\w])_([^_\n]+)_(?![\w])", "<i>$1</i>");
            return text.Replace("\\n", "\n");
        }

        private void Show(Box box, string text)
        {
            box.Text.text = Format(text);
            box.Root.SetActive(true);
            box.Fade(this, 1f);
        }

        private void Hide(Box box)
        {
            if (box.Root.activeSelf)
                box.Fade(this, 0f);
        }

        // ------------------------------------------------------------------
        // UI
        // ------------------------------------------------------------------

        private class Box
        {
            public GameObject Root;
            public TextMeshProUGUI Text;
            public CanvasGroup Group;
            private Coroutine fading;

            public void Fade(MonoBehaviour host, float target)
            {
                if (fading != null)
                    host.StopCoroutine(fading);
                fading = host.StartCoroutine(FadeTo(target));
            }

            private IEnumerator FadeTo(float target)
            {
                float start = Group.alpha;
                for (float t = 0; t < FadeSeconds; t += Time.unscaledDeltaTime)
                {
                    Group.alpha = Mathf.Lerp(start, target, t / FadeSeconds);
                    yield return null;
                }
                Group.alpha = target;
                if (target == 0f)
                    Root.SetActive(false);
            }
        }

        private void BuildUI()
        {
            var canvasObject = new GameObject("CaptionCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900; // above the app, below the command console
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            caption = MakeBox(canvasObject.transform, "Caption", bottom: true, fontSize: 40, width: 1400, fontStyle: FontStyles.Normal);
            title = MakeBox(canvasObject.transform, "Title", bottom: false, fontSize: 60, width: 1500, fontStyle: FontStyles.Bold);

            waitHint = new GameObject("WaitHint", typeof(RectTransform), typeof(TextMeshProUGUI));
            waitHint.transform.SetParent(canvasObject.transform, false);
            var hint = waitHint.GetComponent<TextMeshProUGUI>();
            hint.text = "Click or → to continue";
            hint.fontSize = 34;
            hint.fontStyle = FontStyles.Bold;
            hint.color = new Color(1f, 0.85f, 0.3f, 1f);
            hint.alignment = TextAlignmentOptions.BottomRight;
            hint.raycastTarget = false;
            var hintRect = (RectTransform)waitHint.transform;
            hintRect.anchorMin = hintRect.anchorMax = hintRect.pivot = new Vector2(1, 0);
            hintRect.anchoredPosition = new Vector2(-30, 24);
            hintRect.sizeDelta = new Vector2(520, 50);
            waitHint.SetActive(false);
        }

        private static Box MakeBox(Transform parent, string name, bool bottom, float fontSize, float width, FontStyles fontStyle)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.transform.SetParent(parent, false);

            var background = root.GetComponent<Image>();
            background.color = new Color(0.03f, 0.04f, 0.07f, 0.72f);
            background.raycastTarget = false;

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;

            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(36, 36, 22, 24);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            root.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rect = (RectTransform)root.transform;
            float y = bottom ? 0 : 1;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, y);
            rect.anchoredPosition = new Vector2(0, bottom ? 70 : -40);
            rect.sizeDelta = new Vector2(width, 0);

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(root.transform, false);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = true;
            text.richText = true;
            text.raycastTarget = false;

            root.SetActive(false);
            return new Box { Root = root, Text = text, Group = group };
        }
    }
}
