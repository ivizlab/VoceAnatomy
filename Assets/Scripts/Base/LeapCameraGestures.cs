using Leap;
using Leap.Unity;
using UnityEngine;

/// <summary>
/// Two-Leap-hand camera manipulation: one-hand closed-fist grab + move to rotate, two-hand
/// move/spread to pan and zoom -- the Leap equivalents of right-mouse-drag, middle-mouse-
/// drag, and scroll-wheel zoom in CameraController. Pan and zoom are both driven by raw hand
/// position (no pinch required -- pinch strength proved unreliable to hold through a full
/// arm-spread on this hardware, so a pinch-based zoom design was tried and dropped; see
/// docs/LEAP_INTEGRATION_TODO.md), disambiguated per frame by comparing which signal actually
/// moved more: the hands' midpoint (pan) vs. their separation (zoom), both measured in the
/// same real-world meters so they're directly comparable -- an imperfect/diagonal
/// pull-apart still reads as zoom instead of bleeding into pan. Rotate uses a distinct hand
/// pose (fist) so it never collides with the thumb-index pinch used for clicking; with two
/// hands present at all, LeapInputProvider suppresses hand[0]'s own click so an incidental
/// finger curl during pan/zoom doesn't fire a spurious click. Only active while
/// HandInputCoordinator.Mode == Leap. Two hands present always takes priority over one-hand
/// grab, so grabbing with one hand while the other rests in view won't fight pan/zoom.
/// </summary>
public class LeapCameraGestures : MonoBehaviour
{
    [Header("Rotate: one hand, closed fist (grab) + move")]
    [SerializeField] private float grabThreshold = 0.85f;

    [Header("Pan/zoom: two hands present, moved/spread (disambiguated per frame)")]
    [SerializeField] private float panSensitivity = 1f;
    // 3x the original 2000 -- physical hand separation range on this hardware is
    // limited (hands go off-sensor in X before reaching a full zoom sweep), so the
    // same range of motion needs to cover more zoom distance per meter of separation.
    [SerializeField] private float zoomSensitivity = 6000f;

    /// <summary>True while two hands are present (a pan or zoom gesture may be in progress) --
    /// read by LeapInputProvider so hand[0]'s own pinch-to-click doesn't also fire a spurious
    /// click at whatever the pointer happens to be over while the user is mid pan/zoom.</summary>
    public static bool TwoHandGestureActive { get; private set; }

    // Same interaction-volume normalization LeapInputProvider uses for the cursor,
    // duplicated here (rather than shared) since this maps hand position to a
    // screen-space-equivalent point for rotate/pan math, which may want its own tuning
    // independent of the pointer's volume. See LeapInputProvider for why there's no
    // InteractionBox API in this plugin version.
    [Header("Interaction volume (meters, relative to the controller's own axes)")]
    [SerializeField] private float horizontalRange = 0.15f;
    [SerializeField] private float minHeight = 0.10f;
    [SerializeField] private float maxHeight = 0.40f;

    private LeapServiceProvider _provider;
    private Camera _cam;

    private bool _wasGrabbing;
    private Vector2 _lastGrabScreenPos;

    private bool _wasTwoHand;
    private Vector2 _lastMidpointScreenPos;
    private Vector3 _lastMidpointWorld;
    private float _lastHandSeparation;

    private void Start()
    {
        _cam = Camera.main;
    }

    private Vector2 ToScreenPos(Vector3 palmPosition)
    {
        Vector3 local = _provider.transform.InverseTransformPoint(palmPosition);
        float nx = Mathf.InverseLerp(-horizontalRange, horizontalRange, local.x);
        float ny = Mathf.InverseLerp(minHeight, maxHeight, local.y);
        return new Vector2(nx * Screen.width, ny * Screen.height);
    }

    private void Update()
    {
        bool active = HandInputCoordinator.Mode == HandInputMode.Leap;

        if (_provider == null)
            _provider = FindObjectOfType<LeapServiceProvider>();

        if (!active || _provider == null || CameraController.instance == null)
        {
            _wasGrabbing = false;
            _wasTwoHand = false;
            TwoHandGestureActive = false;
            return;
        }

        Frame frame = _provider.CurrentFrame;
        var hands = frame != null ? frame.Hands : null;
        int handCount = hands != null ? hands.Count : 0;

        // Two hands present: pan (midpoint moved) or zoom (separation changed), whichever
        // signal is larger this frame wins. Takes priority over one-hand grab-rotate.
        if (handCount >= 2)
        {
            _wasGrabbing = false;
            TwoHandGestureActive = true;

            Vector3 palmA = hands[0].PalmPosition;
            Vector3 palmB = hands[1].PalmPosition;
            Vector3 midpointWorld = (palmA + palmB) / 2f;
            float separation = Vector3.Distance(palmA, palmB);
            Vector2 posA = ToScreenPos(palmA);
            Vector2 posB = ToScreenPos(palmB);
            Vector2 midpointScreen = (posA + posB) / 2f;

            if (_wasTwoHand)
            {
                float midpointDeltaWorld = Vector3.Distance(midpointWorld, _lastMidpointWorld);
                float separationDelta = separation - _lastHandSeparation;

                if (Mathf.Abs(separationDelta) > midpointDeltaWorld)
                {
                    if (separationDelta != 0f)
                        CameraController.instance.ApplyGestureZoomDelta(separationDelta * zoomSensitivity);
                }
                else if (midpointScreen != _lastMidpointScreenPos)
                {
                    Vector3 worldPrev = _cam.ScreenToWorldPoint(new Vector3(_lastMidpointScreenPos.x, _lastMidpointScreenPos.y, 0));
                    Vector3 worldCurr = _cam.ScreenToWorldPoint(new Vector3(midpointScreen.x, midpointScreen.y, 0));
                    CameraController.instance.ApplyGesturePanDelta((worldPrev - worldCurr) * panSensitivity);
                }
            }

            _lastMidpointWorld = midpointWorld;
            _lastMidpointScreenPos = midpointScreen;
            _lastHandSeparation = separation;
            _wasTwoHand = true;
            return;
        }

        _wasTwoHand = false;
        TwoHandGestureActive = false;

        // One hand: closed-fist grab + move rotates.
        if (handCount == 1)
        {
            Hand hand = hands[0];
            bool grabbing = hand.GrabStrength > grabThreshold;
            Vector2 screenPos = ToScreenPos(hand.PalmPosition);

            if (grabbing)
            {
                if (_wasGrabbing)
                    CameraController.instance.ApplyGestureRotateDelta(screenPos - _lastGrabScreenPos);
                _lastGrabScreenPos = screenPos;
            }

            _wasGrabbing = grabbing;
            return;
        }

        _wasGrabbing = false;
    }
}
