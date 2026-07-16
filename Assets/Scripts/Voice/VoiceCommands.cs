using Assets.Scripts.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using Vosk;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Push-to-talk voice commands for the presenter demo. Hold <see cref="triggerKey"/>
    /// to listen, release to stop -- recognition audio is only captured while the key
    /// is held, so ambient room conversation is never processed. Uses Vosk (offline,
    /// bundled model in StreamingAssets) instead of Windows Speech so behavior is
    /// identical across machines regardless of per-machine speech settings.
    /// Recognized transcripts are matched by keyword (fuzzily, to tolerate small
    /// mis-transcriptions) against the vocabulary below and only ever call the
    /// app's existing show/hide/reset functions -- never manipulate scene
    /// objects directly.
    /// </summary>
    [RequireComponent(typeof(VoiceProcessor))]
    public class VoiceCommands : MonoBehaviour
    {
        [Header("Push-to-talk trigger")]
        public Key triggerKey = Key.RightCtrl;

        [Tooltip("Keeps the mic capturing for this long after the key is released, so the tail of the last word isn't cut off before Vosk finalizes it. Increase if the last word of a phrase keeps getting dropped; decrease if commands feel laggy.")]
        public float trailingCaptureSeconds = 0.6f;

        [Header("UI")]
        public VoiceListeningIndicator listeningIndicator;

        [Header("Vosk model")]
        [Tooltip("Folder name inside Assets/StreamingAssets/VoskModel containing the unpacked Vosk model.")]
        public string modelFolderName = "vosk-model-small-en-us-0.15";

        private VoiceProcessor voiceProcessor;
        private Model model;
        private VoskRecognizer recognizer;
        private bool voiceReady;
        private bool listening;
        private bool capturing;
        private Coroutine stoppingCoroutine;
        private List<VoiceCommandRule> rules;
        private int captureSampleRate = 16000;
        private string lastPartialText = "";
        private List<GameObject> lastShownObjects;

        private class VoiceCommandRule
        {
            public string Label;
            // All groups must match (AND); any word within a group matches it (OR).
            public string[][] RequiredWordGroups;
            // Exactly one of these is set. ParamAction rules also require a number
            // (digit or spelled out, e.g. "twenty") somewhere in the transcript; if
            // none is found, the rule is skipped rather than treated as a match --
            // see MatchAndExecute.
            public Action Action;
            public Action<int> ParamAction;
        }

        private void Awake()
        {
            voiceProcessor = GetComponent<VoiceProcessor>();
            BuildVocabulary();
            StartCoroutine(InitVosk());
        }

        private IEnumerator InitVosk()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[VoiceCommands] No microphone detected -- voice input disabled. App will run normally otherwise.");
                yield break;
            }

            string modelPath = Path.Combine(Application.streamingAssetsPath, "VoskModel", modelFolderName);

            if (!Directory.Exists(modelPath))
            {
                Debug.LogWarning("[VoiceCommands] Vosk model folder not found at " + modelPath + " -- voice input disabled.");
                yield break;
            }

            // Some mics (webcams especially) don't actually support 16kHz -- Unity silently
            // clamps to whatever the device supports, so we must tell Vosk the real rate or
            // the audio comes in time-warped and never matches anything.
            voiceProcessor.UpdateDevices();
            Microphone.GetDeviceCaps(voiceProcessor.CurrentDeviceName, out int minFreq, out int maxFreq);
            captureSampleRate = (minFreq == 0 && maxFreq == 0) ? 16000 : Mathf.Clamp(16000, minFreq, maxFreq);
            Debug.Log($"[VoiceCommands] Capturing microphone \"{voiceProcessor.CurrentDeviceName}\" at {captureSampleRate} Hz.");

            bool failed = false;
            try
            {
                Vosk.Vosk.SetLogLevel(-1);
                model = new Model(modelPath);
                recognizer = new VoskRecognizer(model, captureSampleRate);
                recognizer.SetMaxAlternatives(0);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[VoiceCommands] Failed to load Vosk model: " + e.Message);
                failed = true;
            }

            if (failed)
                yield break;

            voiceProcessor.OnFrameCaptured += OnFrameCaptured;
            voiceReady = true;
            Debug.Log("[VoiceCommands] Vosk model loaded -- ready for push-to-talk.");
        }

        private void Update()
        {
            if (!voiceReady || Keyboard.current == null)
                return;

            var key = Keyboard.current[triggerKey];
            if (key.wasPressedThisFrame)
                StartListening();
            else if (key.wasReleasedThisFrame)
                StopListening();
        }

        private void StartListening()
        {
            if (listening)
                return;

            // A key-up/key-down cycle faster than the trailing capture window below
            // (e.g. a quick re-press) should discard the previous utterance rather
            // than finalize it late against a freshly-reset recognizer.
            if (stoppingCoroutine != null)
            {
                StopCoroutine(stoppingCoroutine);
                stoppingCoroutine = null;
            }

            listening = true;
            capturing = true;
            lastPartialText = "";
            recognizer.Reset();
            voiceProcessor.StartRecording(captureSampleRate, 512, false);
            listeningIndicator?.Show();
            Debug.Log("[VoiceCommands] Listening...");
        }

        private void StopListening()
        {
            if (!listening)
                return;
            listening = false;
            listeningIndicator?.Hide();
            stoppingCoroutine = StartCoroutine(FinishListening());
        }

        private IEnumerator FinishListening()
        {
            // Keep the mic capturing briefly after key-up before stopping it and
            // finalizing -- releasing the key right as the last word ends otherwise
            // truncates it, since any audio not yet read/fed to Vosk at the moment
            // StopRecording() runs is lost.
            yield return new WaitForSecondsRealtime(trailingCaptureSeconds);

            capturing = false;
            voiceProcessor.StopRecording();

            string transcript = ExtractText(recognizer.FinalResult());
            if (string.IsNullOrWhiteSpace(transcript) && !string.IsNullOrWhiteSpace(lastPartialText))
                transcript = lastPartialText;
            Debug.Log($"[VoiceCommands] Heard: \"{transcript}\"");

            MatchAndExecute(transcript);
            stoppingCoroutine = null;
        }

        private void OnFrameCaptured(short[] samples)
        {
            if (!capturing || recognizer == null)
                return;
            recognizer.AcceptWaveform(samples, samples.Length);

            // Vosk's FinalResult() can come back empty if released right as speech ends,
            // before the decoder commits the utterance -- keep the latest partial as a fallback.
            string partialText = ExtractText(recognizer.PartialResult());
            if (!string.IsNullOrWhiteSpace(partialText))
                lastPartialText = partialText;
        }

        private static string ExtractText(string resultJson)
        {
            var result = new RecognitionResult(resultJson);
            return result.Phrases.Length > 0 ? result.Phrases[0].Text : "";
        }

        private void MatchAndExecute(string transcript)
        {
            if (string.IsNullOrWhiteSpace(transcript))
            {
                Debug.Log("[VoiceCommands] Nothing recognized.");
                return;
            }

            // Split on "and"/"then" so one utterance can chain multiple commands, e.g.
            // "show the heart and focus on it". Capped at 3 segments so a rambling or
            // misheard sentence can't fire an unbounded chain of actions.
            string[] segments = Regex.Split(transcript, @"\band\b|\bthen\b", RegexOptions.IgnoreCase)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Take(3)
                .ToArray();

            if (segments.Length <= 1)
            {
                ExecuteOne(transcript);
                return;
            }

            Debug.Log($"[VoiceCommands] Compound command, {segments.Length} parts: {string.Join(" | ", segments)}");
            StartCoroutine(ExecuteCompound(segments));
        }

        private IEnumerator ExecuteCompound(string[] segments)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                ExecuteOne(segments[i]);
                if (i < segments.Length - 1)
                    yield return new WaitForSeconds(0.4f);
            }
        }

        private void ExecuteOne(string transcript)
        {
            string[] orderedWords = Regex.Matches(transcript.ToLower(), "[a-z']+").Select(m => m.Value).ToArray();
            var words = new HashSet<string>(orderedWords);

            foreach (var rule in rules)
            {
                if (!rule.RequiredWordGroups.All(group => group.Any(candidate => WordGroupContains(words, candidate))))
                    continue;

                if (rule.ParamAction != null)
                {
                    if (!TryParseNumber(orderedWords, out int value))
                    {
                        // Trigger words matched but no number was heard -- keep looking;
                        // a later, non-numeric rule (e.g. plain "zoom in") may still fit.
                        Debug.Log($"[VoiceCommands] \"{rule.Label}\" trigger words matched but no number heard -- checking other rules.");
                        continue;
                    }
                    Debug.Log($"[VoiceCommands] Matched \"{rule.Label}\" with value {value} -> executing.");
                    rule.ParamAction.Invoke(value);
                    return;
                }

                Debug.Log($"[VoiceCommands] Matched \"{rule.Label}\" -> executing.");
                rule.Action.Invoke();
                return;
            }

            Debug.Log($"[VoiceCommands] No command matched for \"{transcript}\".");
        }

        // Number words Vosk's small model transcribes (it does not emit digits for
        // spoken numbers). Combined with digit-string parsing below to also accept
        // literal "10" if a future model or typed-command path ever produces one.
        private static readonly Dictionary<string, int> NumberWords = new Dictionary<string, int>
        {
            { "zero", 0 }, { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 },
            { "five", 5 }, { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 },
            { "ten", 10 }, { "eleven", 11 }, { "twelve", 12 }, { "thirteen", 13 }, { "fourteen", 14 },
            { "fifteen", 15 }, { "sixteen", 16 }, { "seventeen", 17 }, { "eighteen", 18 }, { "nineteen", 19 },
            { "twenty", 20 }, { "thirty", 30 }, { "forty", 40 }, { "fifty", 50 },
            { "sixty", 60 }, { "seventy", 70 }, { "eighty", 80 }, { "ninety", 90 }, { "hundred", 100 },
        };

        // Finds the first run of number words/digits in the transcript and combines
        // them into a single value, e.g. "rotate by twenty five degrees" -> 25,
        // "zoom in one hundred percent" -> 100.
        private static bool TryParseNumber(string[] words, out int value)
        {
            bool found = false;
            int current = 0;

            foreach (var word in words)
            {
                if (int.TryParse(word, out int digit))
                {
                    current += digit;
                    found = true;
                    continue;
                }

                if (NumberWords.TryGetValue(word, out int wordValue))
                {
                    found = true;
                    current = wordValue == 100 ? Math.Max(current, 1) * 100 : current + wordValue;
                    continue;
                }

                if (found)
                    break; // number-word run ended
            }

            value = current;
            return found;
        }

        // Matches a vocabulary word against the heard words, tolerating small
        // mis-transcriptions (e.g. Vosk hearing "move" instead of "remove") via
        // edit distance instead of requiring a hand-maintained list of known
        // confusions. Short words (<=3 letters, e.g. "in"/"out") require an
        // exact match -- fuzzy-matching common short words causes false
        // positives (e.g. "add" vs "and").
        private static bool WordGroupContains(HashSet<string> heardWords, string candidate)
        {
            if (heardWords.Contains(candidate))
                return true;
            if (candidate.Length <= 3)
                return false;

            int threshold = candidate.Length <= 5 ? 1 : 2;
            foreach (var heard in heardWords)
            {
                if (Math.Abs(heard.Length - candidate.Length) > threshold)
                    continue;
                if (LevenshteinDistance(heard, candidate) <= threshold)
                    return true;
            }
            return false;
        }

        private static int LevenshteinDistance(string a, string b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[a.Length, b.Length];
        }

        private void OnDestroy()
        {
            if (voiceProcessor != null)
                voiceProcessor.OnFrameCaptured -= OnFrameCaptured;
            recognizer?.Dispose();
            model?.Dispose();
        }

        // ------------------------------------------------------------------
        // Vocabulary -- add or rename presenter phrases here only. Every
        // rule calls an existing app function; none touch scene objects
        // directly. Word groups are AND'ed; words within a group are OR'd,
        // so e.g. AddLayerRule("muscles", ...) fires on any transcript that
        // contains ("muscle" OR "muscles") AND ("show" OR "add"). Matching
        // is fuzzy (see WordGroupContains), so close mis-transcriptions of
        // these words (e.g. "muscl") also match -- no need to list every
        // near-miss spelling by hand.
        // ------------------------------------------------------------------

        private void BuildVocabulary()
        {
            rules = new List<VoiceCommandRule>();

            // Reset ladder, most specific first. "remove slice"/"reset view" must be
            // checked before the bare "reset" safety net below, since both phrases
            // also contain the word "reset" and would otherwise be shadowed by it.
            rules.Add(Rule("remove slice", new[] { new[] { "slice", "cross" }, new[] { "remove", "reset" } },
                () => CrossSections.Instance.VoiceRemoveSlice()));
            rules.Add(Rule("clear", new[] { new[] { "clear" } }, VoiceClear));
            rules.Add(Rule("reset view", new[] { new[] { "reset" }, new[] { "view", "camera" } },
                () => CameraController.instance.ResetCamera()));

            // Safety net: bare "reset"/"whole body"/"start over" catch anything not
            // matched above -- e.g. "reset everything" is just "reset" with filler
            // ("everything") already tolerated for free by keyword-presence matching.
            rules.Add(Rule("reset", new[] { new[] { "reset" } }, ResetAll));
            rules.Add(Rule("whole body", new[] { new[] { "whole" }, new[] { "body" } }, ResetAll));
            rules.Add(Rule("start over", new[] { new[] { "start" }, new[] { "over" } }, ResetAll));

            AddLayerRule("muscles", new[] { "muscle", "muscles" }, () => GlobalVariables.Instance.muscles);
            AddLayerRule("skeleton", new[] { "skeleton", "bone", "bones" }, () => GlobalVariables.Instance.bones);
            AddLayerRule("nerves", new[] { "nerve", "nerves" }, () => GlobalVariables.Instance.nerves);
            AddLayerRule("organs", new[] { "organ", "organs" }, () => GlobalVariables.Instance.viscera);
            AddLayerRule("blood vessels", new[] { "vessel", "vessels", "artery", "arteries", "vein", "veins" },
                () => GlobalVariables.Instance.arteries.Concat(GlobalVariables.Instance.veins).ToList());
            AddLayerRule("lymph", new[] { "lymph", "lymphatic", "lymphatics" }, () => GlobalVariables.Instance.lymphs);
            AddLayerRule("joints", new[] { "joint", "joints" }, () => GlobalVariables.Instance.joints);

            AddHeroOrganRule("heart", "heart");
            AddHeroOrganRule("brain", "brain");
            AddHeroOrganRule("lungs", "lung", "lungs");
            AddHeroOrganRule("liver", "liver");
            AddHeroOrganRule("stomach", "stomach");

            rules.Add(Rule("focus", new[] { new[] { "focus" } }, FocusOnLastShown));
            rules.Add(Rule("front", new[] { new[] { "front" } }, () => CameraController.instance.ResetOrientation()));
            rules.Add(Rule("pan up", new[] { new[] { "pan" }, new[] { "up" } }, () => CameraController.instance.PanByPercent(15)));
            rules.Add(Rule("pan down", new[] { new[] { "pan" }, new[] { "down" } }, () => CameraController.instance.PanByPercent(-15)));

            // Numbered zoom rules are checked first: if "zoom in/out" is heard with a
            // number (e.g. "zoom in 10 percent"), it wins; if the same trigger words are
            // heard with no number, MatchAndExecute falls through to the plain rules below.
            rules.Add(ParamRule("zoom in by percent", new[] { new[] { "zoom" }, new[] { "in" } },
                value => CameraController.instance.ZoomByPercent(-value)));
            rules.Add(ParamRule("zoom out by percent", new[] { new[] { "zoom" }, new[] { "out" } },
                value => CameraController.instance.ZoomByPercent(value)));
            rules.Add(Rule("zoom in", new[] { new[] { "zoom" }, new[] { "in" } }, FocusOnLastShown));
            rules.Add(Rule("zoom out", new[] { new[] { "zoom" }, new[] { "out" } }, ZoomOut));

            // Same fall-through pattern for rotation: "rotate by 20 degrees" wins over
            // the plain full-spin rules below when a number is heard.
            rules.Add(ParamRule("rotate by degrees", new[] { new[] { "rotate", "turn", "spin" } },
                value => CameraController.instance.RotateBy(value)));

            // "slow rotate" must be checked before the plain "rotate" rule, since it would
            // also match "slow rotate" (first matching rule wins).
            rules.Add(Rule("slow rotate", new[] { new[] { "slow" }, new[] { "rotate", "turn", "spin" } },
                () => CameraController.instance.StartAutoRotate(4f)));
            rules.Add(Rule("rotate", new[] { new[] { "rotate", "turn", "spin" } },
                () => CameraController.instance.StartAutoRotate(2f)));

            // Cross-section (front-to-back slice). 0% = front, 100% = back. "slice at N"
            // (absolute) is checked before plain "slice N" (relative), since "at" is the
            // only thing distinguishing the two; "slice back" and "slow slice" are checked
            // before the plain "slice on" fallback, same fall-through pattern as above.
            rules.Add(ParamRule("slice to percent", new[] { new[] { "slice", "cross" }, new[] { "at" } },
                value => CrossSections.Instance.VoiceJumpTo(value)));
            rules.Add(ParamRule("slice deeper by percent", new[] { new[] { "slice", "cross" } },
                value => CrossSections.Instance.VoiceStepDeeper(value)));
            rules.Add(Rule("slice back", new[] { new[] { "slice", "cross" }, new[] { "back" } },
                () => CrossSections.Instance.VoiceStepBack()));
            rules.Add(Rule("slow slice", new[] { new[] { "slow" }, new[] { "slice", "cross" } },
                () => CrossSections.Instance.VoiceSlowSlice()));
            rules.Add(Rule("slice on", new[] { new[] { "slice", "cross" } },
                () => CrossSections.Instance.VoiceSliceOn()));

            // "gestures on/off" must be two separate rules (not one ParamRule) since
            // "on"/"off" aren't numbers -- the trigger word group differs per rule
            // ("on"/"enable" vs "off"/"disable") so hearing either never matches both.
            rules.Add(Rule("gestures on", new[] { new[] { "gesture", "gestures" }, new[] { "on", "enable" } },
                () => GestureInputProvider.SetEnabled(true)));
            rules.Add(Rule("gestures off", new[] { new[] { "gesture", "gestures" }, new[] { "off", "disable" } },
                () => GestureInputProvider.SetEnabled(false)));
        }

        /// <summary>"clear" / "clear effects" -- cancels the slice and resets orientation, but keeps the current zoom/camera distance.</summary>
        private void VoiceClear()
        {
            CrossSections.Instance.VoiceRemoveSlice();
            CameraController.instance.ResetOrientation();
        }

        private void AddLayerRule(string label, string[] subjectWords, Func<List<TangibleBodyPart>> getParts)
        {
            rules.Add(Rule("show " + label, new[] { subjectWords, new[] { "show", "add" } }, () => ShowParts(getParts())));
            rules.Add(Rule("remove " + label, new[] { subjectWords, new[] { "remove", "hide" } }, () => HideParts(getParts())));
        }

        private void AddHeroOrganRule(string label, params string[] subjectWords)
        {
            rules.Add(Rule("show " + label, new[] { subjectWords, new[] { "show", "add" } }, () => ShowStructure(label)));
        }

        private static VoiceCommandRule Rule(string label, string[][] groups, Action action)
        {
            return new VoiceCommandRule { Label = label, RequiredWordGroups = groups, Action = action };
        }

        private static VoiceCommandRule ParamRule(string label, string[][] groups, Action<int> action)
        {
            return new VoiceCommandRule { Label = label, RequiredWordGroups = groups, ParamAction = action };
        }

        private void ShowParts(List<TangibleBodyPart> parts)
        {
            List<GameObject> shown = new List<GameObject>();
            foreach (var part in parts)
            {
                if (part.gameObject.activeInHierarchy)
                    continue;
                part.transform.SetActiveParentsRecursively(true);
                shown.Add(part.gameObject);
            }
            ActionControl.Instance.AddCommand(new ShowCommand(shown), true);
            Lexicon.Instance.UpdateTreeViewCheckboxes();

            // lastShownObjects drives "focus" -- it must be everything belonging to
            // this layer, not just what "shown" tracked (objects already visible
            // before this command are deliberately left out of "shown", since that
            // list is also used for undo history; but "focus" needs the full set,
            // otherwise it has nothing reliable to select and falls back to framing
            // everything currently on screen).
            lastShownObjects = parts.Select(p => p.gameObject).ToList();
        }

        private void HideParts(List<TangibleBodyPart> parts)
        {
            List<GameObject> hidden = parts.Where(it => it.gameObject.activeInHierarchy).Select(it => it.gameObject).ToList();
            ActionControl.Instance.AddCommand(new DeleteCommand(hidden), true);
        }

        private void ShowStructure(string name)
        {
            Transform found = GlobalVariables.Instance.globalParent.transform.RecursiveFindChild(name);
            if (found == null)
            {
                Debug.LogWarning($"[VoiceCommands] Could not find a structure named '{name}'.");
                return;
            }

            List<GameObject> shown = new List<GameObject>();
            found.SetActiveParentsRecursively(true, shown);
            found.SetActiveRecursively(true, shown);

            ActionControl.Instance.AddCommand(new ShowCommand(shown), false);
            SelectedObjectsManagement.Instance.GetActiveObjects();
            Lexicon.Instance.UpdateTreeViewCheckboxes();

            // lastShownObjects drives "focus" -- use everything under the found
            // structure, not just "shown" (objects already visible before this
            // command, e.g. because an overlapping layer was already on, are left
            // out of "shown" since that list also feeds the undo command; "focus"
            // needs the complete structure or it has nothing reliable to select).
            // Falls back to the found node itself if it has no TangibleBodyPart
            // descendants, so this is never left empty.
            var structureObjects = found.GetComponentsInChildren<TangibleBodyPart>(true)
                .Select(t => t.gameObject)
                .ToList();
            if (structureObjects.Count == 0)
                structureObjects.Add(found.gameObject);
            lastShownObjects = structureObjects;
        }

        private void FocusOnLastShown()
        {
            if (lastShownObjects == null || lastShownObjects.Count == 0)
            {
                Debug.LogWarning("[VoiceCommands] Nothing to focus on yet -- show a structure first.");
                return;
            }

            SelectedObjectsManagement.Instance.DeselectAllObjects();
            foreach (var obj in lastShownObjects)
            {
                if (obj != null)
                    SelectedObjectsManagement.Instance.SelectObject(obj);
            }
            CameraController.instance.CenterView(true);
        }

        private void ZoomOut()
        {
            SelectedObjectsManagement.Instance.DeselectAllObjects();
            CameraController.instance.CenterView(true);
        }

        private void ResetAll()
        {
            ActionControl.Instance.ResetAll();
            lastShownObjects = null;
        }
    }
}
