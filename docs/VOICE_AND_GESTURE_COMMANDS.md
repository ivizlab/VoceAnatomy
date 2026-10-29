# Voice & Gesture Commands Reference

Everything you can say or gesture at the app today. Generated from the actual rule list in
`Assets/Scripts/Voice/VoiceCommandMatcher.cs` and the gesture code in `Assets/Scripts/Base/`,
so it stays accurate to what's implemented -- if you add/change a command or gesture, update
this file alongside the code.

## ASR backend

Recognition runs through the local faster-whisper sidecar (`python/asr_sidecar/`), which the app
launches itself on startup if it isn't already running (voice is ready a few seconds after Play)
-- see its README. Settings (mic, sidecar URL/timeouts, fuzzy matching) live in
`Assets/StreamingAssets/asr_config.json`, so they can be tuned without a recompile. (The earlier
Vosk backend was removed in Oct 2026 -- its accuracy wasn't good enough to keep as a fallback.)

## Push-to-talk

Hold **V** *or* the **mouse middle button** and speak, release when done -- either
one works independently (e.g. keep a small wireless mouse near the Leap controller and use
its middle button instead of reaching for the keyboard). Recognition only runs while a
trigger is held (plus a short trailing window so the last word isn't cut off) -- ambient
room conversation is never processed. Note: the mouse's middle button is also used for
camera pan when dragging with the mouse -- holding it near the Leap without moving the
mouse is fine, but avoid dragging while holding it for push-to-talk. See
`LEAP_INTEGRATION_TODO.md` (in this folder) for other hands-free options considered (wired USB button,
gamepad, etc.).

(Previously Left Ctrl, then Space -- both already do other things in this app: Left Ctrl is
a selection-tool modifier key, and Space triggers "isolate and labels" on the selected part.
V is unused by any other shortcut in the scene.)

You can chain up to 3 commands in one breath with "and"/"then", e.g. *"show the heart and
focus on it"*.

## Voice commands

### Reset / clear
| Say | Does |
|---|---|
| "clear" | Cancels any active cross-section slice and resets camera orientation, but **keeps** current zoom |
| "reset view" / "reset camera" | Full camera reset -- rotation, pan, and zoom all back to default |
| "reset" / "whole body" / "start over" | Full app reset (clears selections, slices, view -- everything) |

### Show / hide anatomy layers
Say **"show"** or **"add"** + the layer name to show it; **"remove"** or **"hide"** + the
layer name to hide it.

| Layer | Trigger words |
|---|---|
| Muscles | "muscle" / "muscles" |
| Skeleton | "skeleton" / "bone" / "bones" |
| Nerves | "nerve" / "nerves" |
| Organs | "organ" / "organs" |
| Blood vessels | "vessel(s)" / "artery" / "arteries" / "vein" / "veins" |
| Lymph | "lymph" / "lymphatic" / "lymphatics" |
| Joints | "joint" / "joints" |

### Show specific organs (show only, no hide variant)
"show" + one of: **heart**, **brain**, **lungs**, **liver**, **stomach**

### Camera / view
| Say | Does |
|---|---|
| "focus" | Frames/selects whatever was last shown (a layer or organ) |
| "front" / "face front" | Resets orientation only -- keeps current zoom/pan |
| "pan up" | Pans camera up 15% |
| "pan down" | Pans camera down 15% |
| "zoom in" | Focuses on last shown structure |
| "zoom in [N] percent" | Zooms in by N% (e.g. "zoom in 20 percent") |
| "zoom out" | Deselects everything and centers/frames the full view |
| "zoom out [N] percent" | Zooms out by N% |
| "rotate" / "turn" / "spin" | Starts auto-rotate (normal speed) |
| "slow rotate" / "slow turn" / "slow spin" | Starts auto-rotate (slow) |
| "rotate/turn/spin by [N] degrees" | Rotates by an exact amount, then stops |

### Cross-section slicing
| Say | Does |
|---|---|
| "slice on" / "cross on" | Enables cross-section slicing |
| "slice back" / "cross back" | Steps the slice plane back |
| "slow slice" / "slow cross" | Slow step-through slice mode |
| "slice at [N]" / "cross at [N]" | Jumps to an absolute depth, 0% = front, 100% = back |
| "slice [N]" / "cross [N]" (no "at") | Steps deeper by N% (relative) |
| "remove slice" / "reset slice" / "remove cross" / "reset cross" | Cancels the active slice |

### Hand-tracking mode switch
| Say | Does |
|---|---|
| "gesture off" / "gestures off" / "gesture disable" | Hand tracking off -- mouse only |
| "gesture leap" / "gestures leap" | Leap Motion controller drives the pointer |
| "gesture video" / "gestures video" / "gesture camera" | Webcam (MediaPipe) drives the pointer |

Numbers can be spoken as words ("twenty five") or digits; the app understands both.
Recognition is fuzzy on longer words (small mis-transcriptions like "muscl" still match),
but short words (3 letters or fewer, e.g. "in"/"out") must be heard exactly.

## Hand gestures

### Leap Motion mode (say "gesture leap" first)
| Gesture | Does |
|---|---|
| Move one hand above the controller | Moves the pointer/cursor (small square, follows your hand) |
| Pinch thumb + index finger | Click / select -- works on both 3D body parts and UI buttons. Cursor turns green while pinching, red otherwise |
| One hand, closed fist, then move | Rotates the camera (like right-mouse-drag) |
| Two hands present, moved together | Pans the camera (like middle-mouse-drag) |
| Two hands present, spread apart or brought together | Zooms the camera -- spread apart to zoom in, bring together to zoom out. The app automatically figures out whether you're panning or zooming based on which motion is larger that frame, so you don't need a separate gesture to switch between them |

Camera gestures (rotate/pan/zoom) are Leap-only -- not available in Video mode.

### Video (webcam) mode (say "gesture video" first)
| Gesture | Does |
|---|---|
| Move your hand in front of the webcam | Moves the pointer/cursor |
| Pinch thumb + index finger | Click / select |

No camera-manipulation gestures in this mode -- use the mouse for rotate/pan/zoom, or
switch to "gesture leap".

### Gesture off ("gesture off")
Hand tracking fully disabled -- mouse and keyboard only.
