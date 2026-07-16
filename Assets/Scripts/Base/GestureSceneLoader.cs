using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads the MediaPipe hand-tracking sample scene additively alongside MainScene so its
/// camera/GPU/HandLandmarkerRunner setup keeps running without duplicating that plumbing.
/// </summary>
public class GestureSceneLoader : MonoBehaviour
{
    [SerializeField] private string gestureSceneName = "Hand Landmark Detection";

    private void Start()
    {
        if (!SceneManager.GetSceneByName(gestureSceneName).isLoaded)
            SceneManager.LoadScene(gestureSceneName, LoadSceneMode.Additive);
    }
}
