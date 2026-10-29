using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// Typed command console: backtick opens/closes it, Enter runs the line through
    /// <see cref="CommandRunner"/> (exactly as voice and macros run), up/down recall history, Tab completes.
    /// The log shows every command from every source -- including the voice trace (heard -> normalized ->
    /// command) -- so it doubles as the debugging view. Builds its own overlay canvas, so dropping this
    /// component on any GameObject is the whole setup. Its input is a TMP_InputField, so
    /// Shortcuts.UserIsWriting() already stops V, Space and the other hotkeys firing while typing.
    /// </summary>
    public class CommandConsole : MonoBehaviour
    {
        public Key toggleKey = Key.Backquote;
        public bool startOpen;

        [Tooltip("Fraction of the screen height the console covers, from the top.")]
        [Range(0.2f, 0.9f)] public float heightFraction = 0.45f;
        public int fontSize = 20;

        [Tooltip("Log lines kept (older ones are dropped).")]
        public int maxLogLines = 200;

        private const string HistoryPrefsKey = "VoceAnatomy.CommandConsole.History";
        private const int MaxHistory = 100;
        // Lines past what fits are clipped by the panel's mask; this only bounds the text TMP lays out.
        private const int RenderedLogLines = 60;

        private GameObject panel;
        private TMP_InputField input;
        private TextMeshProUGUI logText;

        private readonly List<string> logLines = new List<string>();
        private bool logDirty;
        private List<string> history = new List<string>();
        private int historyIndex;
        private bool focusNextFrame;
        private bool caretToEndNextFrame;

        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            BuildUI();
            LoadHistory();
            panel.SetActive(false);
            if (startOpen)
                Open();
        }

        private void OnEnable() => CommandLog.Line += OnLogLine;
        private void OnDisable() => CommandLog.Line -= OnLogLine;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard[toggleKey].wasPressedThisFrame)
            {
                if (IsOpen) Close(); else Open();
                return;
            }
            if (!IsOpen)
                return;

            if (focusNextFrame)
            {
                focusNextFrame = false;
                Focus();
            }
            if (caretToEndNextFrame)
            {
                caretToEndNextFrame = false;
                input.MoveTextEnd(false);
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }
            if (!input.isFocused)
                return;

            if (keyboard.upArrowKey.wasPressedThisFrame)
                RecallHistory(-1);
            else if (keyboard.downArrowKey.wasPressedThisFrame)
                RecallHistory(+1);
            else if (keyboard.tabKey.wasPressedThisFrame)
                Complete();
        }

        private void LateUpdate()
        {
            if (logDirty && IsOpen)
            {
                logDirty = false;
                logText.text = string.Join("\n", logLines.Skip(Math.Max(0, logLines.Count - RenderedLogLines)));
            }
        }

        public void Open()
        {
            panel.SetActive(true);
            logDirty = true;
            // Focusing on the next frame keeps the backtick that opened the console out of the field.
            focusNextFrame = true;
        }

        public void Close()
        {
            input.DeactivateInputField();
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == input.gameObject)
                EventSystem.current.SetSelectedGameObject(null);
            panel.SetActive(false);
        }

        private void Focus()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(input.gameObject);
            input.ActivateInputField();
        }

        private void OnSubmit(string text)
        {
            input.text = "";
            focusNextFrame = true; // TMP drops focus on submit; keep typing without clicking back in.
            if (string.IsNullOrWhiteSpace(text))
                return;

            AddToHistory(text.Trim());
            if (CommandRunner.Instance == null)
            {
                CommandLog.Error("no CommandRunner in the scene");
                return;
            }
            CommandRunner.Instance.Submit(text, CommandSource.Console);
        }

        // ------------------------------------------------------------------
        // Log
        // ------------------------------------------------------------------

        private void OnLogLine(string text, CommandLogLevel level)
        {
            string body = "<noparse>" + text.Replace("</noparse>", "") + "</noparse>";
            switch (level)
            {
                case CommandLogLevel.Echo: body = "<color=#8FCBFF>&gt; " + body + "</color>"; break;
                case CommandLogLevel.Trace: body = "<color=#C3B1FF>voice: " + body + "</color>"; break;
                case CommandLogLevel.Warning: body = "<color=#FFD36B>" + body + "</color>"; break;
                case CommandLogLevel.Error: body = "<color=#FF8080>" + body + "</color>"; break;
                default: body = "<color=#D8DEE6>" + body + "</color>"; break;
            }

            logLines.Add(body);
            if (logLines.Count > maxLogLines)
                logLines.RemoveRange(0, logLines.Count - maxLogLines);
            logDirty = true;
        }

        // ------------------------------------------------------------------
        // History -- per-viewer convenience, so PlayerPrefs (never required for anything to work).
        // ------------------------------------------------------------------

        private void AddToHistory(string line)
        {
            history.Remove(line);
            history.Add(line);
            if (history.Count > MaxHistory)
                history.RemoveRange(0, history.Count - MaxHistory);
            historyIndex = history.Count;
            SaveHistory();
        }

        private void RecallHistory(int step)
        {
            if (history.Count == 0)
                return;
            historyIndex = Mathf.Clamp(historyIndex + step, 0, history.Count);
            input.text = historyIndex < history.Count ? history[historyIndex] : "";
            caretToEndNextFrame = true; // TMP moves the caret on up/down after this runs
        }

        private void LoadHistory()
        {
            try
            {
                string saved = PlayerPrefs.GetString(HistoryPrefsKey, "");
                history = saved.Split('\n').Where(l => l.Length > 0).ToList();
            }
            catch (Exception)
            {
                history = new List<string>();
            }
            historyIndex = history.Count;
        }

        private void SaveHistory()
        {
            try
            {
                PlayerPrefs.SetString(HistoryPrefsKey, string.Join("\n", history));
            }
            catch (Exception)
            {
                // History is a convenience; losing it is fine.
            }
        }

        // ------------------------------------------------------------------
        // Tab completion -- completes the segment after the last ";" against verbs, macro names, and the
        // arguments verbs commonly take. Part names join this list with the part resolver.
        // ------------------------------------------------------------------

        private void Complete()
        {
            string text = input.text;
            int segmentStart = text.LastIndexOf(';') + 1;
            string leading = text.Substring(0, segmentStart);
            string segment = text.Substring(segmentStart).TrimStart();
            string typed = segment.ToLowerInvariant();

            var matches = CompletionCandidates().Where(c => c.StartsWith(typed)).Distinct().OrderBy(c => c).ToList();
            if (matches.Count == 0)
                return;

            string completion = matches.Count == 1 ? matches[0] + " " : CommonPrefix(matches);
            if (matches.Count > 1)
                CommandLog.Info(string.Join("   ", matches.Take(24)) + (matches.Count > 24 ? "   ..." : ""));

            string spacer = segmentStart > 0 ? " " : "";
            input.text = leading + spacer + completion;
            caretToEndNextFrame = true;
        }

        private static IEnumerable<string> CompletionCandidates()
        {
            var macros = MacroLibrary.Names.ToList();
            foreach (var name in CommandRegistry.AllNames)
                yield return name;
            foreach (var macro in macros)
            {
                yield return macro;
                yield return "run " + macro;
            }
            foreach (var system in BodySystems.Names)
            {
                yield return "show " + system;
                yield return "hide " + system;
            }
        }

        private static string CommonPrefix(List<string> items)
        {
            string prefix = items[0];
            foreach (var item in items)
            {
                int i = 0;
                while (i < prefix.Length && i < item.Length && prefix[i] == item[i])
                    i++;
                prefix = prefix.Substring(0, i);
            }
            return prefix;
        }

        // ------------------------------------------------------------------
        // UI, built in code so the console needs no prefab or scene wiring.
        // ------------------------------------------------------------------

        private void BuildUI()
        {
            var canvasObject = new GameObject("CommandConsoleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // above every app panel
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            panel = new GameObject("ConsolePanel", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            panel.transform.SetParent(canvasObject.transform, false);
            panel.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.9f);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0, 1 - heightFraction);
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            const float inputHeight = 44f;
            const float margin = 12f;

            var logObject = new GameObject("Log", typeof(RectTransform), typeof(TextMeshProUGUI));
            logObject.transform.SetParent(panel.transform, false);
            logText = logObject.GetComponent<TextMeshProUGUI>();
            logText.fontSize = fontSize;
            logText.richText = true;
            logText.alignment = TextAlignmentOptions.BottomLeft;
            logText.overflowMode = TextOverflowModes.Overflow;
            logText.raycastTarget = false;
            var logRect = (RectTransform)logObject.transform;
            logRect.anchorMin = Vector2.zero;
            logRect.anchorMax = Vector2.one;
            logRect.offsetMin = new Vector2(margin + 4, inputHeight + margin * 1.5f);
            logRect.offsetMax = new Vector2(-margin - 4, -margin);

            var inputObject = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            inputObject.name = "CommandInput";
            inputObject.transform.SetParent(panel.transform, false);
            inputObject.GetComponent<Image>().color = new Color(0.13f, 0.15f, 0.2f, 1f);
            var inputRect = (RectTransform)inputObject.transform;
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = new Vector2(1, 0);
            inputRect.pivot = new Vector2(0.5f, 0);
            inputRect.offsetMin = new Vector2(margin, margin);
            inputRect.offsetMax = new Vector2(-margin, margin + inputHeight);

            input = inputObject.GetComponent<TMP_InputField>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.textComponent.fontSize = fontSize + 2;
            input.textComponent.color = Color.white;
            input.caretColor = Color.white;
            input.customCaretColor = true;
            input.onFocusSelectAll = false;
            input.resetOnDeActivation = false;
            if (input.placeholder is TextMeshProUGUI placeholder)
            {
                placeholder.text = "type a command -- help lists them, Tab completes, Esc or ` closes";
                placeholder.fontSize = fontSize + 2;
                placeholder.color = new Color(1, 1, 1, 0.35f);
            }
            // The toggle key and Tab are handled by the console, never typed into the field.
            input.onValidateInput = (text, index, c) => c == '`' || c == '\t' ? '\0' : c;
            input.onSubmit.AddListener(OnSubmit);
        }
    }
}
