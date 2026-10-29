/// <summary>
/// Which hand-tracking backend, if any, drives PointerInput's mouse-equivalent state.
/// Video is the existing MediaPipe webcam pipeline (GestureInputProvider); Leap is the
/// Leap Motion controller (LeapInputProvider). Only one is ever active at a time.
/// </summary>
public enum HandInputMode
{
    Off,
    Video,
    Leap
}

/// <summary>
/// Single source of truth for the current HandInputMode, set at runtime by the
/// "gesture off"/"gesture leap"/"gesture video" voice commands in VoiceCommands.
/// GestureInputProvider and LeapInputProvider both read <see cref="Mode"/> each frame
/// to decide whether they're allowed to drive the pointer.
/// </summary>
public static class HandInputCoordinator
{
    // Defaults to Video so behavior is unchanged from before this mode selector existed
    // (GestureInputProvider's own Inspector checkbox defaulted to "gestures on").
    public static HandInputMode Mode { get; private set; } = HandInputMode.Video;

    public static void SetMode(HandInputMode mode)
    {
        Mode = mode;
    }
}
