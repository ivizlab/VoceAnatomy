using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single seam RaycastObject reads pointer position/click through. Falls back to the
/// physical mouse whenever no hand-tracking backend is both enabled (per
/// HandInputCoordinator.Mode) and currently tracking a hand.
/// </summary>
public static class PointerInput
{
    private static bool UseLeap => LeapInputProvider.Enabled && LeapInputProvider.IsTracking;
    private static bool UseMediaPipe => GestureInputProvider.Enabled && GestureInputProvider.IsTracking;

    /// <summary>True while either hand-tracking backend is actively driving the pointer -- used
    /// by RaycastObject to show/hide the on-screen gesture cursor regardless of which backend.</summary>
    public static bool IsHandTracking => UseLeap || UseMediaPipe;

    public static Vector2 Position => UseLeap ? LeapInputProvider.ScreenPosition
        : UseMediaPipe ? GestureInputProvider.ScreenPosition
        : Mouse.current.position.ReadValue();

    public static bool WasPressedThisFrame => UseLeap ? LeapInputProvider.PinchWasPressedThisFrame
        : UseMediaPipe ? GestureInputProvider.PinchWasPressedThisFrame
        : Mouse.current.leftButton.wasPressedThisFrame;

    public static bool WasReleasedThisFrame => UseLeap ? LeapInputProvider.PinchWasReleasedThisFrame
        : UseMediaPipe ? GestureInputProvider.PinchWasReleasedThisFrame
        : Mouse.current.leftButton.wasReleasedThisFrame;

    /// <summary>True while the pointer's current backend is actively pinching/pressed --
    /// used by RaycastObject to color the on-screen gesture cursor.</summary>
    public static bool IsPinching => UseLeap ? LeapInputProvider.IsPinching
        : UseMediaPipe ? GestureInputProvider.IsPinching
        : Mouse.current.leftButton.isPressed;
}
