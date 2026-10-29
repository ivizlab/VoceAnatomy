using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws all 21 hand landmarks (joints + bone connections) of up to two hands as UI dots/lines
/// while gesture tracking is active - purely visual, toggleable, for demo purposes. Tinted by
/// GestureInputProvider.ActiveGesture so it's visible when a fist rotate / two-fist pan-zoom has
/// engaged. Reads the same HandLandmarkerRunner as GestureInputProvider, independently, using
/// the same GestureInputProvider.LandmarkToScreen mapping so it lines up with the pointer.
/// </summary>
public class HandSkeletonView : MonoBehaviour
{
    private static readonly (int a, int b)[] Connections =
    {
        (0,1),(1,2),(2,3),(3,4),
        (0,5),(5,6),(6,7),(7,8),
        (5,9),(9,10),(10,11),(11,12),
        (9,13),(13,14),(14,15),(15,16),
        (13,17),(17,18),(18,19),(19,20),
        (0,17)
    };

    private const int MaxHands = GestureInputProvider.MaxHands;
    private const int LandmarkCount = GestureInputProvider.LandmarkCount;

    [SerializeField] private bool showSkeleton = true;
    [SerializeField] private RectTransform parent;
    [SerializeField] private Color dotColor = Color.cyan;
    [SerializeField] private Color lineColor = new Color(0f, 1f, 1f, 0.6f);
    [SerializeField] private Color rotateColor = new Color(1f, 0.6f, 0f, 0.9f);
    [SerializeField] private Color panZoomColor = new Color(1f, 0.2f, 1f, 0.9f);
    [SerializeField] private float dotSize = 14f;
    [SerializeField] private float lineThickness = 4f;

    private HandLandmarkerRunner _runner;

    // Only plain floats cross from the MediaPipe worker thread - see GestureInputProvider for why
    // (MediaPipe reuses/overwrites its result buffers, so holding a reference and reading it
    // later on the main thread can race with that overwrite).
    private readonly object _pendingLock = new object();
    private bool _newResultAvailable;
    private int _pendingHandCount;
    private readonly float[] _pendingX = new float[MaxHands * LandmarkCount];
    private readonly float[] _pendingY = new float[MaxHands * LandmarkCount];

    private int _handCount;
    private readonly float[] _x = new float[MaxHands * LandmarkCount];
    private readonly float[] _y = new float[MaxHands * LandmarkCount];
    private float _lastResultTime;
    private const float TrackingTimeout = 0.3f;

    private GameObject _root;
    private GameObject[] _handRoots;
    private RectTransform[] _dots;
    private RectTransform[] _lines;
    private Image[] _lineImages;
    private readonly Vector2[] _points = new Vector2[LandmarkCount];

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
                    }
                    count++;
                }
            }
            _pendingHandCount = count;
            _newResultAvailable = true;
        }
    }

    private void EnsureBuilt()
    {
        if (_root != null || parent == null)
            return;

        _root = new GameObject("HandSkeletonDots", typeof(RectTransform));
        _root.transform.SetParent(parent, false);

        _handRoots = new GameObject[MaxHands];
        _lines = new RectTransform[MaxHands * Connections.Length];
        _lineImages = new Image[MaxHands * Connections.Length];
        _dots = new RectTransform[MaxHands * LandmarkCount];
        for (int h = 0; h < MaxHands; h++)
        {
            _handRoots[h] = new GameObject("Hand" + h, typeof(RectTransform));
            _handRoots[h].transform.SetParent(_root.transform, false);

            for (int i = 0; i < Connections.Length; i++)
            {
                int li = h * Connections.Length + i;
                _lines[li] = CreateUIElement(_handRoots[h].transform, "Bone", lineColor, new Vector2(1f, lineThickness));
                _lineImages[li] = _lines[li].GetComponent<Image>();
            }

            for (int i = 0; i < LandmarkCount; i++)
                _dots[h * LandmarkCount + i] = CreateUIElement(_handRoots[h].transform, "Joint", dotColor, new Vector2(dotSize, dotSize));
        }
    }

    private RectTransform CreateUIElement(Transform parentTransform, string goName, Color color, Vector2 size)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parentTransform, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        return rt;
    }

    private void Update()
    {
        if (_runner == null)
        {
            _runner = FindObjectOfType<HandLandmarkerRunner>();
            if (_runner != null)
                _runner.OnResult += HandleResult;
        }

        lock (_pendingLock)
        {
            if (_newResultAvailable)
            {
                _handCount = _pendingHandCount;
                System.Array.Copy(_pendingX, _x, _handCount * LandmarkCount);
                System.Array.Copy(_pendingY, _y, _handCount * LandmarkCount);
                _lastResultTime = Time.realtimeSinceStartup;
                _newResultAvailable = false;
            }
        }

        bool stale = Time.realtimeSinceStartup - _lastResultTime > TrackingTimeout;
        bool visible = showSkeleton && parent != null && _handCount > 0 && !stale
            && HandInputCoordinator.Mode == HandInputMode.Video;

        if (!visible)
        {
            if (_root != null)
                _root.SetActive(false);
            return;
        }

        EnsureBuilt();
        _root.SetActive(true);

        Color bone = GestureInputProvider.ActiveGesture == VideoHandGesture.Rotate ? rotateColor
            : GestureInputProvider.ActiveGesture == VideoHandGesture.PanZoom ? panZoomColor
            : lineColor;

        // Hand order here can differ from GestureInputProvider's (it re-orders for stable
        // slots) - doesn't matter, both hands get the same tint.
        for (int h = 0; h < MaxHands; h++)
        {
            bool shown = h < _handCount;
            _handRoots[h].SetActive(shown);
            if (!shown)
                continue;

            for (int i = 0; i < LandmarkCount; i++)
            {
                _points[i] = GestureInputProvider.LandmarkToScreen(_x[h * LandmarkCount + i], _y[h * LandmarkCount + i]);
                _dots[h * LandmarkCount + i].position = _points[i];
            }

            for (int i = 0; i < Connections.Length; i++)
            {
                var (a, b) = Connections[i];
                Vector2 pa = _points[a];
                Vector2 pb = _points[b];
                int li = h * Connections.Length + i;
                var line = _lines[li];
                line.position = (pa + pb) / 2f;
                line.sizeDelta = new Vector2(Vector2.Distance(pa, pb), lineThickness);
                line.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(pb.y - pa.y, pb.x - pa.x) * Mathf.Rad2Deg);
                _lineImages[li].color = bone;
            }
        }
    }

    private void OnDestroy()
    {
        if (_runner != null)
            _runner.OnResult -= HandleResult;
    }
}
