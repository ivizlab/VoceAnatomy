using Assets.Scripts.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Push-to-talk voice commands for the presenter demo. Hold <see cref="triggerKey"/>
    /// (or the mouse middle button -- a second, independent trigger, e.g. for a wireless
    /// mouse kept near the Leap controller) to listen, release to stop -- recognition
    /// audio is only captured while a trigger is held, so ambient room conversation is
    /// never processed.
    ///
    /// This class is now just the four pipeline stages wired together, each independently
    /// testable/swappable:
    /// capture (push-to-talk timing + <see cref="VoiceProcessor"/>, unchanged) -> transcribe
    /// (<see cref="IAsrBackend"/>, the Whisper sidecar configured by <see cref="AsrConfig"/>) -> normalize +
    /// match (<see cref="TranscriptNormalizer"/> + <see cref="VoiceCommandMatcher"/>, pure text
    /// in/labels out, no scene dependency) -> execute (<see cref="VoiceCommandMatcher.Execute"/>
    /// against the <see cref="VoiceCommandActions"/> built in <see cref="BuildActions"/>).
    /// </summary>
    [RequireComponent(typeof(VoiceProcessor))]
    public class VoiceCommands : MonoBehaviour
    {
        [Header("Push-to-talk trigger")]
        public Key triggerKey = Key.V;

        [Tooltip("Keeps the mic capturing for this long after the key is released, so the tail of the last word isn't cut off before the ASR backend finalizes it. Increase if the last word of a phrase keeps getting dropped; decrease if commands feel laggy.")]
        public float trailingCaptureSeconds = 0.6f;

        [Header("UI")]
        public VoiceListeningIndicator listeningIndicator;

        private VoiceProcessor voiceProcessor;
        private AsrConfig config;
        private IAsrBackend asrBackend;
        private VoiceCommandMatcher matcher;

        private bool voiceReady;
        private bool listening;
        private bool capturing;
        private Coroutine stoppingCoroutine;
        private int captureSampleRate = 16000;
        private List<GameObject> lastShownObjects;

        private void Awake()
        {
            voiceProcessor = GetComponent<VoiceProcessor>();
            config = AsrConfig.Load(Path.Combine(Application.streamingAssetsPath, "asr_config.json"));
            matcher = new VoiceCommandMatcher(BuildActions()) { FuzzyMatchingEnabled = config.FuzzyMatchingEnabled };
            StartCoroutine(InitAsr());
        }

        private IEnumerator InitAsr()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[VoiceCommands] No microphone detected -- voice input disabled. App will run normally otherwise.");
                yield break;
            }

            ResolveMicrophoneDevice();

            // Some mics (webcams especially) don't actually support 16kHz -- Unity silently
            // clamps to whatever the device supports, so backends that need 16kHz (Whisper)
            // resample from whatever we actually captured at.
            Microphone.GetDeviceCaps(voiceProcessor.CurrentDeviceName, out int minFreq, out int maxFreq);
            captureSampleRate = (minFreq == 0 && maxFreq == 0) ? 16000 : Mathf.Clamp(16000, minFreq, maxFreq);
            Debug.Log($"[VoiceCommands] Capturing microphone \"{voiceProcessor.CurrentDeviceName}\" at {captureSampleRate} Hz.");

            // Export the vocabulary this matcher just built, so the Whisper sidecar's
            // initial_prompt is generated from the same single source of truth as the matcher
            // rather than a hand-maintained duplicate.
            matcher.ExportVocabulary(Path.Combine(Application.streamingAssetsPath, "asr_vocabulary.json"));

            var whisper = new WhisperAsrBackend(config.WhisperBaseUrl, config.WhisperTimeoutSeconds,
                config.AutoStartSidecar ? config.SidecarDirectory : null, config.SidecarStartupTimeoutSeconds);
            // Assigned before Initialize() so OnDestroy still shuts down a sidecar this
            // backend launched if play mode ends while it's still warming up.
            asrBackend = whisper;
            yield return whisper.Initialize();

            if (!asrBackend.IsReady)
            {
                Debug.LogWarning("[VoiceCommands] Whisper ASR backend failed to become ready -- voice input disabled. App will run normally otherwise.");
                yield break;
            }

            voiceProcessor.OnFrameCaptured += OnFrameCaptured;
            voiceReady = true;
            Debug.Log("[VoiceCommands] Whisper backend ready -- listening for push-to-talk.");
        }

        // Selects the mic by name substring rather than Unity's serialized device index --
        // USB device indices reorder between reboots/unplug cycles, which would otherwise
        // silently break a demo. Empty configured name keeps whatever device index
        // VoiceProcessor already defaults to.
        private void ResolveMicrophoneDevice()
        {
            voiceProcessor.UpdateDevices();

            if (!string.IsNullOrWhiteSpace(config.MicrophoneNameContains))
            {
                int index = voiceProcessor.Devices.FindIndex(
                    d => d.IndexOf(config.MicrophoneNameContains, StringComparison.OrdinalIgnoreCase) >= 0);

                if (index >= 0)
                {
                    if (index != voiceProcessor.CurrentDeviceIndex)
                        voiceProcessor.ChangeDevice(index);
                }
                else
                {
                    Debug.LogWarning($"[VoiceCommands] No microphone device name contains \"{config.MicrophoneNameContains}\" -- using default device.");
                }
            }

            Debug.Log($"[VoiceCommands] Resolved microphone device: \"{voiceProcessor.CurrentDeviceName}\".");
        }

        private void Update()
        {
            if (!voiceReady)
                return;

            // Two independent trigger sources (keyboard key, mouse middle-click) both hold
            // push-to-talk active -- combined as "is either currently held" rather than
            // separate press/release edges, so releasing one while the other is still held
            // (e.g. tapping the mouse button mid-sentence while holding the key) doesn't cut
            // listening off early.
            bool triggerHeld = false;
            if (Keyboard.current != null)
                triggerHeld |= Keyboard.current[triggerKey].isPressed;
            if (Mouse.current != null)
                triggerHeld |= Mouse.current.middleButton.isPressed;

            if (triggerHeld && !listening)
                StartListening();
            else if (!triggerHeld && listening)
                StopListening();
        }

        private void StartListening()
        {
            if (listening)
                return;

            // A key-up/key-down cycle faster than the trailing capture window below
            // (e.g. a quick re-press) should discard the previous utterance rather
            // than finalize it late against a freshly-reset backend.
            if (stoppingCoroutine != null)
            {
                StopCoroutine(stoppingCoroutine);
                stoppingCoroutine = null;
            }

            listening = true;
            capturing = true;
            asrBackend.ResetUtterance();
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
            // truncates it, since any audio not yet delivered to the backend at the moment
            // StopRecording() runs is lost.
            yield return new WaitForSecondsRealtime(trailingCaptureSeconds);

            capturing = false;
            voiceProcessor.StopRecording();

            AsrResult asrResult = AsrResult.Fail("no result");
            yield return asrBackend.FinalizeUtterance(result => asrResult = result);

            if (!asrResult.Success)
            {
                // Never hang, never block input -- log it, give a brief non-blocking visual
                // cue, and return to idle ready state. A missed command is recoverable in a
                // live demo; a frozen app is not.
                Debug.LogWarning($"[VoiceCommands] ASR call failed or timed out: {asrResult.ErrorMessage}");
                StartCoroutine(FlashIndicatorBriefly());
                stoppingCoroutine = null;
                yield break;
            }

            // The raw transcript is the durable record -- log it unaltered, including its
            // original punctuation and casing, before anything derives from it.
            string rawTranscript = asrResult.Text ?? "";
            Debug.Log($"[VoiceCommands] Raw transcript: \"{rawTranscript}\"");

            string normalized = TranscriptNormalizer.Normalize(rawTranscript);
            Debug.Log($"[VoiceCommands] Normalized: \"{normalized}\"");

            var matches = matcher.Match(normalized);
            if (matches.Count == 0)
            {
                Debug.Log("[VoiceCommands] No command matched.");
            }
            else
            {
                Debug.Log("[VoiceCommands] Resolved command(s): " +
                    string.Join(" | ", matches.Select(m => m.Param.HasValue ? $"{m.CommandLabel} ({m.Param})" : m.CommandLabel)));
                StartCoroutine(ExecuteMatches(matches));
            }

            stoppingCoroutine = null;
        }

        private IEnumerator ExecuteMatches(List<VoiceCommandMatcher.MatchResult> matches)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                matcher.Execute(matches[i]);
                if (i < matches.Count - 1)
                    yield return new WaitForSeconds(0.4f);
            }
        }

        private IEnumerator FlashIndicatorBriefly()
        {
            listeningIndicator?.Show();
            yield return new WaitForSeconds(0.75f);
            listeningIndicator?.Hide();
        }

        private void OnFrameCaptured(short[] samples)
        {
            if (!capturing || asrBackend == null)
                return;
            asrBackend.AcceptFrame(samples, captureSampleRate);
        }

        private void OnDestroy()
        {
            if (voiceProcessor != null)
                voiceProcessor.OnFrameCaptured -= OnFrameCaptured;
            asrBackend?.Shutdown();
        }

        // ------------------------------------------------------------------
        // Dispatch -- wires the matcher's fixed set of command labels to the app's existing
        // show/hide/reset functions. Add or rename presenter phrases in VoiceCommandMatcher,
        // not here; this only needs updating if a label there is renamed.
        // ------------------------------------------------------------------

        private VoiceCommandActions BuildActions()
        {
            return new VoiceCommandActions
            {
                RemoveSlice = () => CrossSections.Instance.VoiceRemoveSlice(),
                Clear = VoiceClear,
                ResetView = () => CameraController.instance.ResetCamera(),
                ResetAll = ResetAll,

                ShowMuscles = () => ShowParts(GlobalVariables.Instance.muscles),
                HideMuscles = () => HideParts(GlobalVariables.Instance.muscles),
                ShowSkeleton = () => ShowParts(GlobalVariables.Instance.bones),
                HideSkeleton = () => HideParts(GlobalVariables.Instance.bones),
                ShowNerves = () => ShowParts(GlobalVariables.Instance.nerves),
                HideNerves = () => HideParts(GlobalVariables.Instance.nerves),
                ShowOrgans = () => ShowParts(GlobalVariables.Instance.viscera),
                HideOrgans = () => HideParts(GlobalVariables.Instance.viscera),
                ShowBloodVessels = () => ShowParts(GlobalVariables.Instance.arteries.Concat(GlobalVariables.Instance.veins).ToList()),
                HideBloodVessels = () => HideParts(GlobalVariables.Instance.arteries.Concat(GlobalVariables.Instance.veins).ToList()),
                ShowLymph = () => ShowParts(GlobalVariables.Instance.lymphs),
                HideLymph = () => HideParts(GlobalVariables.Instance.lymphs),
                ShowJoints = () => ShowParts(GlobalVariables.Instance.joints),
                HideJoints = () => HideParts(GlobalVariables.Instance.joints),

                ShowHeart = () => ShowStructure("heart"),
                ShowBrain = () => ShowStructure("brain"),
                ShowLungs = () => ShowStructure("lungs"),
                ShowLiver = () => ShowStructure("liver"),
                ShowStomach = () => ShowStructure("stomach"),

                FocusOnLastShown = FocusOnLastShown,
                ResetOrientation = () => CameraController.instance.ResetOrientation(),
                PanUp = () => CameraController.instance.PanByPercent(15),
                PanDown = () => CameraController.instance.PanByPercent(-15),

                ZoomInByPercent = value => CameraController.instance.ZoomByPercent(-value),
                ZoomOutByPercent = value => CameraController.instance.ZoomByPercent(value),
                ZoomOut = ZoomOut,

                RotateByDegrees = value => CameraController.instance.RotateBy(value),
                SlowRotate = () => CameraController.instance.StartAutoRotate(4f),
                Rotate = () => CameraController.instance.StartAutoRotate(2f),

                SliceToPercent = value => CrossSections.Instance.VoiceJumpTo(value),
                SliceDeeperByPercent = value => CrossSections.Instance.VoiceStepDeeper(value),
                SliceBack = () => CrossSections.Instance.VoiceStepBack(),
                SlowSlice = () => CrossSections.Instance.VoiceSlowSlice(),
                SliceOn = () => CrossSections.Instance.VoiceSliceOn(),

                GestureOff = () => HandInputCoordinator.SetMode(HandInputMode.Off),
                GestureLeap = () => HandInputCoordinator.SetMode(HandInputMode.Leap),
                GestureVideo = () => HandInputCoordinator.SetMode(HandInputMode.Video),
            };
        }

        /// <summary>"clear" / "clear effects" -- cancels the slice and resets orientation, but keeps the current zoom/camera distance.</summary>
        private void VoiceClear()
        {
            CrossSections.Instance.VoiceRemoveSlice();
            CameraController.instance.ResetOrientation();
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
