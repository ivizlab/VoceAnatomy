using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using TMPro;
using UnityEngine;

/// <summary>What the webcam hand(s) are currently doing -- exposed so HandSkeletonView can
/// tint the skeleton as at-a-glance feedback.</summary>
public enum VideoHandGesture
{
    None,
    Pointer,
    Rotate,
    PanZoom
}

/// <summary>
/// Reads up to two hands from the running HandLandmarkerRunner (found automatically, even if it
/// lives in an additively-loaded scene) and turns them into:
/// - Pointer: index fingertip of the first hand + thumb/index pinch, exposed as mouse-equivalent
///   static state that PointerInput reads (unchanged from the original pick-only version).
/// - Rotate: one hand closed into a fist, then moved -- like right-mouse-drag. Open the hand to
///   "let go" and reposition it, close it again to keep turning.
/// - Pan/zoom: two fists -- move both together to pan (middle-mouse-drag), spread them apart /
///   bring them together to zoom (scroll wheel). Whichever signal moved more this frame wins,
///   so a slightly diagonal pull-apart still reads as zoom rather than bleeding into pan.
/// Camera gestures drive CameraController's ApplyGesture*Delta methods, which reuse the mouse
/// path's speed tuning. A fist also brings thumb and index together, so pinch is gated off while
/// any camera gesture is active, an in-progress pinch is cancelled (no release => no click), and
/// pinch stays locked after a gesture until thumb and index have actually separated.
/// Enable/disable is exposed as an Inspector checkbox and can also be driven at runtime via
/// <see cref="SetEnabled"/>. Actual activation additionally requires
/// <see cref="HandInputCoordinator"/>.Mode == Video -- set by the "gesture off"/"gesture
/// leap"/"gesture video" voice commands -- so this backend only drives the pointer while the
/// Leap Motion backend (LeapInputProvider) isn't selected.
/// </summary>
public class GestureInputProvider : MonoBehaviour
{
    public const int MaxHands = 2;
    public const int LandmarkCount = 21;

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
    public static VideoHandGesture ActiveGesture { get; private set; }

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

    // Fist = every finger's tip is close to its own knuckle, measured relative to palm size
    // (wrist to middle knuckle) so it works at any distance from the camera. Uses 3D distance
    // (MediaPipe's relative z) so fingers pointing at the camera don't look short. The index
    // finger is included on purpose: in a pinch it stays long (it forms the "O" with the thumb),
    // which is what keeps a pinch with the other fingers curled from reading as a fist.
    // Enter/exit thresholds differ (hysteresis) so the state doesn't flicker at the boundary.
    [Header("Fist detection (largest fingertip-to-knuckle / palm-size ratio)")]
    [SerializeField] private float fistEnterRatio = 0.5f;
    [SerializeField] private float fistExitRatio = 0.62f;

    [Header("Camera gestures")]
    [SerializeField] private bool enableCameraGestures = true;
    [Tooltip("Pan/zoom only while both hands are fists -- keeps two hands casually in view from moving the camera.")]
    [SerializeField] private bool twoHandRequiresFists = true;
    [SerializeField] private float rotateSensitivity = 1f;
    [SerializeField] private float panSensitivity = 1f;
    // Applied to the change in hand separation as a fraction of screen height, so it's
    // resolution independent. Scroll-wheel-equivalent units (see ApplyGestureZoomDelta).
    [SerializeField] private float zoomSensitivity = 3000f;
    [Tooltip("Higher = snappier, lower = smoother. Webcam landmarks jitter at ~30fps, so some smoothing is needed.")]
    [SerializeField] private float motionSmoothing = 15f;

    [Header("Live on-screen debug readout (optional)")]
    [SerializeField] private bool showDebugReadout = true;
    [SerializeField] private RectTransform debugReadoutParent;

    private static readonly (int knuckle, int tip)[] Fingers = { (5, 8), (9, 12), (13, 16), (17, 20) };
    private static readonly int[] PalmPoints = { 0, 5, 9, 13, 17 };

    private HandLandmarkerRunner _runner;
    private Camera _cam;

    // Only plain floats cross from the MediaPipe worker thread to the main thread. Earlier
    // versions stored the whole HandLandmarkerResult (and its List<T> buffers) and read from it
    // later on the main thread - but MediaPipe reuses/overwrites those same buffers for the next
    // frame, so a delayed read could race with that overwrite. Extracting plain numbers
    // synchronously inside the callback avoids that entirely. The lock keeps the main thread
    // from copying a half-written set of arrays.
    private readonly object _pendingLock = new object();
    private bool _newResultAvailable;
    private int _pendingHandCount;
    private readonly float[] _pendingX = new float[MaxHands * LandmarkCount];
    private readonly float[] _pendingY = new float[MaxHands * LandmarkCount];
    private readonly float[] _pendingZ = new float[MaxHands * LandmarkCount];

    private int _handCount;
    private readonly float[] _x = new float[MaxHands * LandmarkCount];
    private readonly float[] _y = new float[MaxHands * LandmarkCount];
    private readonly float[] _z = new float[MaxHands * LandmarkCount];
    private float _lastResultTime;
    private bool _wasPinching;

    private readonly bool[] _isFist = new bool[MaxHands];
    private readonly float[] _fistRatio = new float[MaxHands];
    private Vector2 _prevWrist0, _prevWrist1;

    private VideoHandGesture _lastGesture;
    private int _rotateHand;
    private Vector2 _smoothPalm, _lastPalm;
    private Vector2 _smoothMid, _lastMid;
    private float _smoothSep, _lastSep;
    // Set when a camera gesture ends; pinch stays off until thumb and index separate, so
    // opening a fist (thumb still near index) doesn't fire a click.
    private bool _pinchLocked;

    private TextMeshProUGUI _debugText;

    // Called from a MediaPipe worker thread (Live Stream mode) - must not touch Unity APIs here.
    private void HandleResult(HandLandmarkerResult result)
    {
        lock (_pendingLock)
        {
            int count = 0;
            if (result.handLandmarks != null)
            {
                for (int h = 0; h < result.handLandmarks.Count && h < MaxHands; h++)
                {
                    var lm = result.handLandmarks[h].landmarks;
                    if (lm == null || lm.Count < LandmarkCount)
                        break;
                    for (int i = 0; i < LandmarkCount; i++)
                    {
                        _pendingX[h * LandmarkCount + i] = lm[i].x;
                        _pendingY[h * LandmarkCount + i] = lm[i].y;
                        _pendingZ[h * LandmarkCount + i] = lm[i].z;
                    }
                    count++;
                }
            }
            _pendingHandCount = count;
            _newResultAvailable = true;
        }
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
        rt.sizeDelta = new Vector2(900f, 200f);
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
        if (_cam == null)
            _cam = Camera.main;

        lock (_pendingLock)
        {
            if (_newResultAvailable)
            {
                _handCount = _pendingHandCount;
                int n = _handCount * LandmarkCount;
                System.Array.Copy(_pendingX, _x, n);
                System.Array.Copy(_pendingY, _y, n);
                System.Array.Copy(_pendingZ, _z, n);
                _lastResultTime = Time.realtimeSinceStartup;
                _newResultAvailable = false;
                KeepHandSlotsStable();
            }
        }

        PinchWasPressedThisFrame = false;
        PinchWasReleasedThisFrame = false;

        bool stale = Time.realtimeSinceStartup - _lastResultTime > trackingTimeout;
        bool validFrame = Enabled && _runner != null && _handCount > 0 && !stale;

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
            _pinchLocked = false;
            _isFist[0] = _isFist[1] = false;
            _lastGesture = ActiveGesture = VideoHandGesture.None;
            if (_debugText != null && Enabled)
                _debugText.text = "Pinch: no hand tracked";
            return;
        }

        ScreenPosition = LandmarkToScreen(_x[8], _y[8]);

        for (int h = 0; h < MaxHands; h++)
            _isFist[h] = h < _handCount && UpdateFist(h);

        var gesture = ChooseGesture();
        ApplyCameraGesture(gesture);

        // Pinch on the first hand, only while nothing else is going on.
        float dx = _x[4] - _x[8];
        float dy = _y[4] - _y[8];
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        bool rawPinch = dist < pinchThreshold;

        if (gesture != VideoHandGesture.Pointer)
            _pinchLocked = true;
        else if (!rawPinch)
            _pinchLocked = false;

        bool isPinching = rawPinch && !_pinchLocked;

        if (isPinching && !_wasPinching)
            PinchWasPressedThisFrame = true;
        else if (!isPinching && _wasPinching && !_pinchLocked)
            PinchWasReleasedThisFrame = true;
        // else: pinch cancelled by a camera gesture -- dropped without a release, so whatever
        // was under the cursor at press time is never clicked.

        _wasPinching = isPinching;
        IsPinching = isPinching;
        IsTracking = true;

        if (_debugText != null && Enabled)
        {
            string fists = $"fist ratio: {_fistRatio[0]:F2}{(_isFist[0] ? " FIST" : "")}";
            if (_handCount > 1)
                fists += $"   |   {_fistRatio[1]:F2}{(_isFist[1] ? " FIST" : "")}";
            _debugText.text = $"Pinch dist: {dist:F3}  (threshold {pinchThreshold:F3})\n{(isPinching ? "PINCHING" : "not pinching")}{(_pinchLocked ? " (locked)" : "")}\n"
                + $"hands: {_handCount}  gesture: {gesture}\n{fists}  (enter <{fistEnterRatio:F2}, exit >{fistExitRatio:F2})";
            _debugText.color = gesture == VideoHandGesture.Pointer ? (isPinching ? Color.green : Color.red) : Color.yellow;
        }
    }

    /// <summary>MediaPipe doesn't promise the same hand stays at the same index between frames.
    /// With two hands, swap the slots if that keeps each wrist closer to where it was, so
    /// per-hand state (fist hysteresis, which hand is rotating) follows the actual hand.</summary>
    private void KeepHandSlotsStable()
    {
        if (_handCount < 2)
        {
            if (_handCount == 1)
                _prevWrist0 = new Vector2(_x[0], _y[0]);
            return;
        }

        var w0 = new Vector2(_x[0], _y[0]);
        var w1 = new Vector2(_x[LandmarkCount], _y[LandmarkCount]);
        float keep = (w0 - _prevWrist0).sqrMagnitude + (w1 - _prevWrist1).sqrMagnitude;
        float swap = (w1 - _prevWrist0).sqrMagnitude + (w0 - _prevWrist1).sqrMagnitude;
        if (swap < keep)
        {
            for (int i = 0; i < LandmarkCount; i++)
            {
                int a = i, b = LandmarkCount + i;
                (_x[a], _x[b]) = (_x[b], _x[a]);
                (_y[a], _y[b]) = (_y[b], _y[a]);
                (_z[a], _z[b]) = (_z[b], _z[a]);
            }
            (w0, w1) = (w1, w0);
        }
        _prevWrist0 = w0;
        _prevWrist1 = w1;
    }

    private float LandmarkDistance(int hand, int a, int b)
    {
        int ia = hand * LandmarkCount + a, ib = hand * LandmarkCount + b;
        float dx = _x[ia] - _x[ib], dy = _y[ia] - _y[ib], dz = _z[ia] - _z[ib];
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private bool UpdateFist(int hand)
    {
        float palm = LandmarkDistance(hand, 0, 9);
        if (palm < 1e-5f)
            return false;

        float maxRatio = 0f;
        foreach (var (knuckle, tip) in Fingers)
            maxRatio = Mathf.Max(maxRatio, LandmarkDistance(hand, knuckle, tip) / palm);
        _fistRatio[hand] = maxRatio;

        return _isFist[hand] ? maxRatio < fistExitRatio : maxRatio < fistEnterRatio;
    }

    private VideoHandGesture ChooseGesture()
    {
        if (!enableCameraGestures || CameraController.instance == null)
            return VideoHandGesture.Pointer;

        if (_handCount >= 2 && (!twoHandRequiresFists || (_isFist[0] && _isFist[1])))
            return VideoHandGesture.PanZoom;

        // One fist (the other hand, if any, is open/resting in view) rotates with the fist.
        if (_isFist[0] != _isFist[1] || (_handCount == 1 && _isFist[0]))
        {
            _rotateHand = _isFist[0] ? 0 : 1;
            return VideoHandGesture.Rotate;
        }

        return VideoHandGesture.Pointer;
    }

    private Vector2 PalmScreen(int hand)
    {
        float sx = 0f, sy = 0f;
        foreach (int i in PalmPoints)
        {
            sx += _x[hand * LandmarkCount + i];
            sy += _y[hand * LandmarkCount + i];
        }
        return LandmarkToScreen(sx / PalmPoints.Length, sy / PalmPoints.Length);
    }

    private void ApplyCameraGesture(VideoHandGesture gesture)
    {
        bool starting = gesture != _lastGesture;
        _lastGesture = ActiveGesture = gesture;
        float t = 1f - Mathf.Exp(-motionSmoothing * Time.deltaTime);

        if (gesture == VideoHandGesture.Rotate)
        {
            Vector2 palm = PalmScreen(_rotateHand);
            if (starting)
            {
                _smoothPalm = _lastPalm = palm;
                return;
            }
            _smoothPalm = Vector2.Lerp(_smoothPalm, palm, t);
            CameraController.instance.ApplyGestureRotateDelta((_smoothPalm - _lastPalm) * rotateSensitivity);
            _lastPalm = _smoothPalm;
        }
        else if (gesture == VideoHandGesture.PanZoom)
        {
            // Midpoint and separation don't depend on which hand is which, so a slot swap
            // can't cause a jump.
            Vector2 a = PalmScreen(0), b = PalmScreen(1);
            Vector2 mid = (a + b) / 2f;
            float sep = Vector2.Distance(a, b);
            if (starting)
            {
                _smoothMid = _lastMid = mid;
                _smoothSep = _lastSep = sep;
                return;
            }
            _smoothMid = Vector2.Lerp(_smoothMid, mid, t);
            _smoothSep = Mathf.Lerp(_smoothSep, sep, t);

            Vector2 midDelta = _smoothMid - _lastMid;
            float sepDelta = _smoothSep - _lastSep;

            // Both in screen pixels, so directly comparable.
            if (Mathf.Abs(sepDelta) > midDelta.magnitude)
            {
                CameraController.instance.ApplyGestureZoomDelta(sepDelta / Screen.height * zoomSensitivity);
            }
            else if (midDelta != Vector2.zero && _cam != null)
            {
                Vector3 worldPrev = _cam.ScreenToWorldPoint(new Vector3(_lastMid.x, _lastMid.y, 0));
                Vector3 worldCurr = _cam.ScreenToWorldPoint(new Vector3(_smoothMid.x, _smoothMid.y, 0));
                CameraController.instance.ApplyGesturePanDelta((worldPrev - worldCurr) * panSensitivity);
            }

            _lastMid = _smoothMid;
            _lastSep = _smoothSep;
        }
    }

    private void OnDestroy()
    {
        if (_runner != null)
            _runner.OnResult -= HandleResult;
        if (_instance == this)
            _instance = null;
    }
}
