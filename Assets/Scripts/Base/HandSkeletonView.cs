using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws all 21 hand landmarks (joints + bone connections) as UI dots/lines while gesture
/// tracking is active - purely visual, toggleable, for demo purposes. Reads the same
/// HandLandmarkerRunner as GestureInputProvider, independently, using the same
/// GestureInputProvider.LandmarkToScreen mapping so it lines up with the pointer.
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

    [SerializeField] private bool showSkeleton = true;
    [SerializeField] private RectTransform parent;
    [SerializeField] private Color dotColor = Color.cyan;
    [SerializeField] private Color lineColor = new Color(0f, 1f, 1f, 0.6f);
    [SerializeField] private float dotSize = 14f;
    [SerializeField] private float lineThickness = 4f;

    private HandLandmarkerRunner _runner;

    // Only plain floats cross from the MediaPipe worker thread - see GestureInputProvider for why
    // (MediaPipe reuses/overwrites its result buffers, so holding a reference and reading it
    // later on the main thread can race with that overwrite).
    private volatile bool _newResultAvailable;
    private volatile bool _pendingHasHand;
    private readonly float[] _pendingX = new float[21];
    private readonly float[] _pendingY = new float[21];

    private bool _hasHand;
    private readonly float[] _x = new float[21];
    private readonly float[] _y = new float[21];
    private float _lastResultTime;
    private const float TrackingTimeout = 0.3f;

    private GameObject _root;
    private RectTransform[] _dots;
    private RectTransform[] _lines;

    private void HandleResult(HandLandmarkerResult result)
    {
        bool hasHand = result.handLandmarks != null && result.handLandmarks.Count > 0
            && result.handLandmarks[0].landmarks != null && result.handLandmarks[0].landmarks.Count >= 21;

        if (hasHand)
        {
            var lm = result.handLandmarks[0].landmarks;
            for (int i = 0; i < 21; i++)
            {
                _pendingX[i] = lm[i].x;
                _pendingY[i] = lm[i].y;
            }
        }
        _pendingHasHand = hasHand;
        _newResultAvailable = true;
    }

    private void EnsureBuilt()
    {
        if (_root != null || parent == null)
            return;

        _root = new GameObject("HandSkeletonDots", typeof(RectTransform));
        _root.transform.SetParent(parent, false);

        _lines = new RectTransform[Connections.Length];
        for (int i = 0; i < Connections.Length; i++)
            _lines[i] = CreateUIElement("Bone", lineColor, new Vector2(1f, lineThickness));

        _dots = new RectTransform[21];
        for (int i = 0; i < 21; i++)
            _dots[i] = CreateUIElement("Joint", dotColor, new Vector2(dotSize, dotSize));
    }

    private RectTransform CreateUIElement(string goName, Color color, Vector2 size)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(_root.transform, false);
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

        if (_newResultAvailable)
        {
            _hasHand = _pendingHasHand;
            if (_hasHand)
            {
                System.Array.Copy(_pendingX, _x, 21);
                System.Array.Copy(_pendingY, _y, 21);
            }
            _lastResultTime = Time.realtimeSinceStartup;
            _newResultAvailable = false;
        }

        bool stale = Time.realtimeSinceStartup - _lastResultTime > TrackingTimeout;
        bool visible = showSkeleton && parent != null && _hasHand && !stale
            && HandInputCoordinator.Mode == HandInputMode.Video;

        if (!visible)
        {
            if (_root != null)
                _root.SetActive(false);
            return;
        }

        EnsureBuilt();
        _root.SetActive(true);

        var points = new Vector2[21];
        for (int i = 0; i < 21; i++)
            points[i] = GestureInputProvider.LandmarkToScreen(_x[i], _y[i]);

        for (int i = 0; i < 21; i++)
            _dots[i].position = points[i];

        for (int i = 0; i < Connections.Length; i++)
        {
            var (a, b) = Connections[i];
            Vector2 pa = points[a];
            Vector2 pb = points[b];
            var line = _lines[i];
            line.position = (pa + pb) / 2f;
            line.sizeDelta = new Vector2(Vector2.Distance(pa, pb), lineThickness);
            line.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(pb.y - pa.y, pb.x - pa.x) * Mathf.Rad2Deg);
        }
    }

    private void OnDestroy()
    {
        if (_runner != null)
            _runner.OnResult -= HandleResult;
    }
}
