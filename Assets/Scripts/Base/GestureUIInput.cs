using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Lets hand-tracking pinch (Leap or MediaPipe) click UI elements, not just 3D colliders.
/// Unity's EventSystem/InputSystemUIInputModule only reads the real Mouse device for its
/// own pointer events, so a synthetic gesture pinch never reached Buttons etc. -- only
/// RaycastObject's own 3D-only physics raycast saw it. This raycasts UI at
/// PointerInput.Position via EventSystem.RaycastAll (covers every registered
/// Canvas/GraphicRaycaster in one call) and fires pointerDown/pointerUp/pointerClick by
/// hand, but only while a hand-tracking backend is actually driving the pointer -- the
/// real mouse continues through Unity's normal path unmodified.
///
/// Covers IPointerClickHandler (UI Button/Toggle/Tab etc.). Does not yet synthesize
/// drag events (IBeginDrag/IDrag/IEndDrag), so gesture-driven dragging of sliders or
/// draggable windows isn't supported -- only clicks/taps.
/// </summary>
public class GestureUIInput : MonoBehaviour
{
    private static readonly List<RaycastResult> Results = new List<RaycastResult>();

    /// <summary>True this frame if the gesture cursor is currently over a UI element --
    /// used by RaycastObject to skip 3D world raycasting/selection while over UI.</summary>
    public static bool IsOverUI { get; private set; }

    private GameObject _pressedObject;

    private void Update()
    {
        if (!PointerInput.IsHandTracking || EventSystem.current == null)
        {
            IsOverUI = false;
            return;
        }

        var pointerData = new PointerEventData(EventSystem.current) { position = PointerInput.Position };

        Results.Clear();
        EventSystem.current.RaycastAll(pointerData, Results);
        IsOverUI = Results.Count > 0;

        if (PointerInput.WasPressedThisFrame)
        {
            _pressedObject = IsOverUI
                ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(Results[0].gameObject)
                : null;

            if (_pressedObject != null)
                ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.pointerDownHandler);
        }
        else if (PointerInput.WasReleasedThisFrame && _pressedObject != null)
        {
            ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(_pressedObject, pointerData, ExecuteEvents.pointerClickHandler);
            _pressedObject = null;
        }
    }
}
