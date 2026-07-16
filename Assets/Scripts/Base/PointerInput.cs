using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single seam RaycastObject reads pointer position/click through. Falls back to the
/// physical mouse whenever gesture input is disabled or no hand is currently tracked.
/// </summary>
public static class PointerInput
{
    private static bool UseGesture => GestureInputProvider.Enabled && GestureInputProvider.IsTracking;

    public static Vector2 Position => UseGesture ? GestureInputProvider.ScreenPosition : Mouse.current.position.ReadValue();

    public static bool WasPressedThisFrame => UseGesture ? GestureInputProvider.PinchWasPressedThisFrame : Mouse.current.leftButton.wasPressedThisFrame;

    public static bool WasReleasedThisFrame => UseGesture ? GestureInputProvider.PinchWasReleasedThisFrame : Mouse.current.leftButton.wasReleasedThisFrame;
}
