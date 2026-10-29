using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using TMPro;
using UnityEngine;

/// <summary>
/// Reads index-fingertip position and thumb/index pinch from the running HandLandmarkerRunner
/// (found automatically, even if it lives in an additively-loaded scene) and exposes them as
/// mouse-equivalent static state that PointerInput reads. Enable/disable is exposed as an
/// Inspector checkbox and can also be driven at runtime via <see cref="SetEnabled"/>. Actual
/// activation additionally requires <see cref="HandInputCoordinator"/>.Mode == Video -- set by
/// the "gesture off"/"gesture leap"/"gesture video" voice commands -- so this backend only
/// drives the pointer while the Leap Motion backend (LeapInputProvider) isn't selected.
/// </summary>
public class GestureInputProvider : MonoBehaviour
{
    public static bool Enabled { get; private set; }

    private static GestureInputProvider _instance;

    /// <summary>Runtime on/off toggle -- mirrors the Inspector checkbox, which stays the source
    /// of truth for the initial state each time the scene loads.</summary>
    public static void SetEnabled(bool enabled)
    {
        if (_instance != null)
            _instance.enableGestures = enabled;
    }

    public static bool MirrorX { get; private set; }
    public static bool IsTracking { get; private set; }
    public static Vector2 ScreenPosition { get; private set; }
    public static bool IsPinching { get; private set; }
    public static bool PinchWasPressedThisFrame { get; private set; }
    public static bool PinchWasReleasedThisFrame { get; private set; }

    // Converts a normalized landmark (plain x/y, 0-1) to screen pixels, applying the mirror
    // setting. Shared with HandSkeletonView so the skeleton lines up with the pointer/pinch.
    public static Vector2 LandmarkToScreen(float x, float y)
    {
        float nx = MirrorX ? 1f - x : x;
        return new Vector2(nx * Screen.width, (1f - y) * Screen.height);
    }

    [SerializeField] private bool enableGestures = true;
    [SerializeField] private bool mirrorX = true;
    [SerializeField] private float trackingTimeout = 0.3f;

    // Plain normalized (0-1) image-space distance between thumb and index tips, no smoothing,
    // single threshold - simplest version, closest to the one that got ~50% reliable pinches
    // earlier. Fancier scale-invariant/smoothed attempts since then made things worse, not
    // better - reverted rather than keep piling on complexity.
    [Header("Pinch detection (normalized 0-1 image-space thumb-index distance, no smoothing)")]
    [SerializeField] private float pinchThreshold = 0.035f;

    [Header("Live on-screen debug readout (optional)")]
    [SerializeField] private bool showDebugReadout = true;
    [SerializeField] private RectTransform debugReadoutParent;

    private HandLandmarkerRunner _runner;

    // Only plain floats cross from the MediaPipe worker thread to the main thread. Earlier
    // versions stored the whole HandLandmarkerResult (and its List<T> buffers) and read from it
    // later on the main thread - but MediaPipe reuses/overwrites those same buffers for the next
    // frame, so a delayed read could race with that overwrite. Extracting plain numbers
    // synchronously inside the callback avoids that entirely.
    private volatile bool _newResultAvailable;
    private volatile bool _pendingHasHand;
    private float _pendingIndexX, _pendingIndexY, _pendingThumbX, _pendingThumbY;

    private bool _hasHand;
    private float _indexX, _indexY, _thumbX, _thumbY;
    private float _lastResultTime;
    private bool _wasPinching;

    private TextMeshProUGUI _debugText;

    // Called from a MediaPipe worker thread (Live Stream mode) - must not touch Unity APIs here.
    private void HandleResult(HandLandmarkerResult result)
    {
        bool hasHand = result.handLandmarks != null && result.handLandmarks.Count > 0
            && result.handLandmarks[0].landmarks != null && result.handLandmarks[0].landmarks.Count > 8;

        if (hasHand)
        {
            var lm = result.handLandmarks[0].landmarks;
            _pendingIndexX = lm[8].x;
            _pendingIndexY = lm[8].y;
            _pendingThumbX = lm[4].x;
            _pendingThumbY = lm[4].y;
        }
        _pendingHasHand = hasHand;
        _newResultAvailable = true;
    }

    private void EnsureDebugTextBuilt()
    {
        if (_debugText != null || debugReadoutParent == null)
            return;

        var go = new GameObject("PinchDebugReadout", typeof(RectTransform));
        go.transform.SetParent(debugReadoutParent, false);
        _debugText = go.AddComponent<TextMeshProUGUI>();
        _debugText.fontSize = 36;
        _debugText.alignment = TextAlignmentOptions.TopLeft;
        var rt = _debugText.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(700f, 100f);
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        Enabled = enableGestures && HandInputCoordinator.Mode == HandInputMode.Video;
        MirrorX = mirrorX;

        if (_runner == null)
        {
            _runner = FindObjectOfType<HandLandmarkerRunner>();
            if (_runner != null)
                _runner.OnResult += HandleResult;
        }

        if (_newResultAvailable)
        {
            _hasHand = _pendingHasHand;
            if (_hasHand)
            {
                _indexX = _pendingIndexX;
                _indexY = _pendingIndexY;
                _thumbX = _pendingThumbX;
                _thumbY = _pendingThumbY;
            }
            _lastResultTime = Time.realtimeSinceStartup;
            _newResultAvailable = false;
        }

        PinchWasPressedThisFrame = false;
        PinchWasReleasedThisFrame = false;

        bool stale = Time.realtimeSinceStartup - _lastResultTime > trackingTimeout;
        bool validFrame = Enabled && _runner != null && _hasHand && !stale;

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

        if (!validFrame)
        {
            IsTracking = false;
            IsPinching = false;
            _wasPinching = false;
            if (_debugText != null && Enabled)
                _debugText.text = "Pinch: no hand tracked";
            return;
        }

        ScreenPosition = LandmarkToScreen(_indexX, _indexY);

        float dx = _thumbX - _indexX;
        float dy = _thumbY - _indexY;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        bool isPinching = dist < pinchThreshold;

        if (_debugText != null && Enabled)
        {
            _debugText.text = $"Pinch dist: {dist:F3}  (threshold {pinchThreshold:F3})\n{(isPinching ? "PINCHING" : "not pinching")}";
            _debugText.color = isPinching ? Color.green : Color.red;
        }

        if (isPinching && !_wasPinching)
            PinchWasPressedThisFrame = true;
        else if (!isPinching && _wasPinching)
            PinchWasReleasedThisFrame = true;

        _wasPinching = isPinching;
        IsPinching = isPinching;
        IsTracking = true;
    }

    private void OnDestroy()
    {
        if (_runner != null)
            _runner.OnResult -= HandleResult;
        if (_instance == this)
            _instance = null;
    }
}
