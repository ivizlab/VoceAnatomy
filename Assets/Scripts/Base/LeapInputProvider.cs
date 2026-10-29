using Leap;
using Leap.Unity;
using TMPro;
using UnityEngine;

/// <summary>
/// Reads palm position and thumb/index pinch from the running Leap.LeapServiceProvider (found
/// automatically, even if it lives in an additively-loaded scene) and exposes them as
/// mouse-equivalent static state that PointerInput reads -- the Leap Motion controller
/// equivalent of GestureInputProvider (MediaPipe webcam). Only active while
/// HandInputCoordinator.Mode == Leap, set by the "gesture off"/"gesture leap"/"gesture video"
/// voice commands.
/// </summary>
public class LeapInputProvider : MonoBehaviour
{
    public static bool Enabled { get; private set; }

    private static LeapInputProvider _instance;

    public static bool MirrorX { get; private set; }
    public static bool IsTracking { get; private set; }
    public static Vector2 ScreenPosition { get; private set; }
    public static bool IsPinching { get; private set; }
    public static bool PinchWasPressedThisFrame { get; private set; }
    public static bool PinchWasReleasedThisFrame { get; private set; }

    [SerializeField] private bool mirrorX = false;
    [SerializeField] private float trackingTimeout = 0.3f;

    // The controller has no fixed image frame like a webcam, so instead of normalized 0-1
    // image coordinates we read the hand's position relative to the LeapServiceProvider's own
    // transform (meters, matching Unity world units for a Desktop-mounted controller sitting
    // flat with its default, unrotated transform) and normalize that against a configurable
    // interaction volume. Tune these in the Inspector to match your physical setup.
    [Header("Interaction volume (meters, relative to the controller's own axes)")]
    [SerializeField] private float horizontalRange = 0.15f;
    [SerializeField] private float minHeight = 0.10f;
    [SerializeField] private float maxHeight = 0.40f;

    // Hysteresis on Leap's own PinchStrength (0 = open hand, 1 = fully pinched): a higher bar
    // to START a pinch (pinchOnThreshold) than to KEEP it going (pinchOffThreshold). A single
    // shared threshold let PinchStrength chatter back and forth across it during a real
    // pinch-hold (observed live: low single-digit-percent click success, worse the longer the
    // hold), splitting one intended pinch into several spurious press/release pairs. The gap
    // between the two thresholds is a dead-band the noisy signal can't chatter across once
    // you're already pinching.
    [Header("Pinch detection (Leap PinchStrength, 0-1, hysteresis)")]
    [SerializeField] private float pinchOnThreshold = 0.75f;
    [SerializeField] private float pinchOffThreshold = 0.5f;

    // Confirmed live (2026-07-17): hysteresis alone wasn't enough -- tracking was fresh every
    // frame (ruling out tracking dropouts) and hand count stayed correctly at 1 (ruling out a
    // phantom second hand), yet a slow held pinch still failed far more than a fast one, and
    // the click action is a select/deselect *toggle* -- so an even number of spurious
    // press/release pairs during one held pinch cancels itself out to no visible change even
    // though clicks did fire. That means PinchStrength itself briefly dips below
    // pinchOffThreshold and back mid-hold (real tremor or algorithmic noise, not a tracking
    // gap). This debounce requires the signal to actually stay below pinchOffThreshold for a
    // short window before a release is committed, so a brief real dip doesn't fragment one
    // intended pinch into several.
    [SerializeField] private float releaseDebounceSeconds = 0.15f;
    private float _belowOffThresholdSince = -1f;

    [Header("Live on-screen debug readout (optional)")]
    [SerializeField] private bool showDebugReadout = false;
    [SerializeField] private RectTransform debugReadoutParent;

    private LeapServiceProvider _provider;
    private bool _wasPinching;
    private float _lastHandTime;
    private bool _everSeenHand;
    private Vector2 _cachedScreenPosition;
    private bool _cachedIsPinching;
    private int _lastHandCount;

    private TextMeshProUGUI _debugText;

    private void EnsureDebugTextBuilt()
    {
        if (_debugText != null || debugReadoutParent == null)
            return;

        // Parented to the Canvas root, not directly to debugReadoutParent -- debugReadoutParent
        // is the GestureCursor square, which RaycastObject deactivates whenever hand-tracking
        // isn't currently active and which moves every frame to follow the pointer. Parenting
        // there made the readout disappear exactly when it's most useful (no hand tracked) and
        // drift around the screen otherwise. debugReadoutParent is only used here to locate the
        // stable Canvas root.
        // includeInactive:true -- debugReadoutParent (the cursor) is frequently inactive at the
        // moment this first runs (before tracking starts), and GetComponentInParent only walks
        // active objects by default, which silently fell through to the broken fallback below.
        var canvasTransform = debugReadoutParent.GetComponentInParent<Canvas>(true)?.transform ?? debugReadoutParent;

        var go = new GameObject("LeapPinchDebugReadout", typeof(RectTransform));
        go.transform.SetParent(canvasTransform, false);
        _debugText = go.AddComponent<TextMeshProUGUI>();
        _debugText.fontSize = 36;
        _debugText.alignment = TextAlignmentOptions.TopLeft;
        var rt = _debugText.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -80f);
        rt.sizeDelta = new Vector2(700f, 180f);
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        Enabled = HandInputCoordinator.Mode == HandInputMode.Leap;
        MirrorX = mirrorX;

        if (_provider == null)
            _provider = FindObjectOfType<LeapServiceProvider>();

        PinchWasPressedThisFrame = false;
        PinchWasReleasedThisFrame = false;

        if (showDebugReadout)
        {
            EnsureDebugTextBuilt();
            if (_debugText != null)
                _debugText.gameObject.SetActive(Enabled);
        }
        else if (_debugText != null)
        {
            _debugText.gameObject.SetActive(false);
        }

        Hand hand = null;
        if (Enabled && _provider != null)
        {
            Frame frame = _provider.CurrentFrame;
            _lastHandCount = frame != null ? frame.Hands.Count : 0;
            if (frame != null && frame.Hands.Count > 0)
                hand = frame.Hands[0];
        }

        if (hand != null)
        {
            _lastHandTime = Time.realtimeSinceStartup;
            _everSeenHand = true;

            Vector3 local = _provider.transform.InverseTransformPoint(hand.PalmPosition);
            float nx = Mathf.InverseLerp(-horizontalRange, horizontalRange, local.x);
            float ny = Mathf.InverseLerp(minHeight, maxHeight, local.y);
            if (mirrorX)
                nx = 1f - nx;

            _cachedScreenPosition = new Vector2(nx * Screen.width, ny * Screen.height);

            // Hysteresis: once _wasPinching is true, only pinchOffThreshold (lower) is needed
            // to stay pinching; starting a new pinch still needs to clear pinchOnThreshold
            // (higher). See field comments above for why.
            float activeThreshold = _wasPinching ? pinchOffThreshold : pinchOnThreshold;
            bool rawPinching = !LeapCameraGestures.TwoHandGestureActive && hand.PinchStrength > activeThreshold;

            if (_wasPinching && !rawPinching)
            {
                // Currently pinching but this frame reads below the off-threshold -- don't
                // commit to a release until it's stayed low for releaseDebounceSeconds.
                if (_belowOffThresholdSince < 0f)
                    _belowOffThresholdSince = Time.realtimeSinceStartup;
                _cachedIsPinching = Time.realtimeSinceStartup - _belowOffThresholdSince < releaseDebounceSeconds;
            }
            else
            {
                _belowOffThresholdSince = -1f;
                _cachedIsPinching = rawPinching;
            }

            if (_debugText != null)
            {
                _debugText.text = $"Leap pinch: {hand.PinchStrength:F2}  (on {pinchOnThreshold:F2} / off {pinchOffThreshold:F2})\n{(_cachedIsPinching ? "PINCHING" : "not pinching")}";
                _debugText.color = _cachedIsPinching ? Color.green : Color.red;
            }
        }

        // A single frame with zero reported hands does not mean tracking was lost -- this
        // hardware routinely misses a frame here and there (motion blur, brief occlusion, the
        // known flakiness logged in docs/LEAP_INTEGRATION_TODO.md) even while a hand is genuinely
        // still in view. Hold the last known position/pinch state through a short grace window
        // (trackingTimeout) instead of instantly invalidating on any single empty frame --
        // mirrors GestureInputProvider's MediaPipe-side handling, which already does this via
        // its own timeout and doesn't require a fresh detection every single frame.
        bool stale = !_everSeenHand || Time.realtimeSinceStartup - _lastHandTime > trackingTimeout;
        bool validFrame = Enabled && !stale;

        if (!validFrame)
        {
            IsTracking = false;
            // Firing the release here (instead of just silently clearing _wasPinching) means a
            // pinch that was in progress when tracking is genuinely lost still closes out
            // cleanly, rather than leaving RaycastObject/GestureUIInput's press state stuck and
            // making the next real pinch look like a fresh, un-paired press.
            if (_wasPinching)
                PinchWasReleasedThisFrame = true;
            IsPinching = false;
            _wasPinching = false;
            if (_debugText != null && Enabled)
                _debugText.text = _provider == null ? "Leap: no LeapServiceProvider in scene" : "Leap: no hand tracked";
            return;
        }

        ScreenPosition = _cachedScreenPosition;
        bool isPinching = _cachedIsPinching;

        if (isPinching && !_wasPinching)
            PinchWasPressedThisFrame = true;
        else if (!isPinching && _wasPinching)
            PinchWasReleasedThisFrame = true;

        _wasPinching = isPinching;
        IsPinching = isPinching;
        IsTracking = true;
    }

    // Runs after every Update() this frame (including RaycastObject's and GestureUIInput's),
    // so PointerInput/GestureUIInput/RaycastObject all reflect this frame's final state here --
    // appends live click-pipeline diagnostics to the same debug readout toggled by
    // showDebugReadout, so a stuck selection can be diagnosed without guessing blind.
    private void LateUpdate()
    {
        if (!Enabled || !showDebugReadout || _debugText == null)
            return;

        float msSinceFreshHand = (Time.realtimeSinceStartup - _lastHandTime) * 1000f;
        _debugText.text += $"\nhands:{_lastHandCount} 2Hgesture:{LeapCameraGestures.TwoHandGestureActive} UI:{GestureUIInput.IsOverUI} press:{PointerInput.WasPressedThisFrame} release:{PointerInput.WasReleasedThisFrame}\nsince fresh hand: {msSinceFreshHand:F0}ms\n{RaycastObject.DebugClickInfo}";
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
