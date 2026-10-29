# Leap Motion Integration — Status & Next Steps

Goal: add Leap Motion controller support (one-hand mouse-move + pinch-select) alongside
the existing MediaPipe webcam gesture system, without breaking MediaPipe. A single
voice-driven mode switch controls which backend (if any) drives the pointer.

## Summary of the big things (see dated sections below for full detail)

- **Selection (pinch-click) is fixed and confirmed working, 80%+ hit rate** (2026-07-17).
  Took five rounds to find; the real root cause turned out to be the gesture cursor's own
  UI `Image` having `Raycast Target` on, so it was raycasting into itself and reporting
  "over UI" everywhere, blocking all 3D selection and hijacking UI clicks (see "Selection
  round 5" below). The other four rounds of fixes (EventSystem gate using the real mouse
  instead of the gesture cursor, tracking-dropout silently swallowing a pinch-release,
  distance-gated click logic that couldn't survive natural hand drift, and pinch-signal
  chatter needing hysteresis + a debounce) were all real, separate bugs worth having fixed,
  but round 5 alone could have masked whether any of them were working.
- **Zoom gesture**: settled on raw two-hand separation change (spread apart = zoom in,
  bring together = zoom out), disambiguated from pan per-frame by comparing which signal
  (hand midpoint movement vs. hand separation change, both in real-world meters) is larger
  that frame. A double-pinch design was tried first and dropped -- unreliable on this
  hardware, see "Round 2" below.
- **Tracking randomly drops after Stop/Play, recovered by flipping the orientation
  setting in the Ultraleap software** -- confirmed *not* caused by this project's code
  (nothing here touches device orientation, and the standalone Ultraleap software was
  affected too, not just Unity). Still not root-caused. New detail (2026-07-17, later):
  the fix is **asymmetric** -- flipping the orientation setting to its *other* value wakes
  tracking back up, but flipping it back again immediately afterward does **not** re-break
  it (i.e. it's not simply "leaving inverted mode" that fixes it). This points more at "any
  settings-change forces the tracking service to reinitialize/reconnect to the device" (a
  generic kick, same effect as the USB-port-swap fixes noted further down) than at
  orientation being functionally meaningful here. Still unresolved -- see the dedicated
  section below for what to check next time it happens.
- **Push-to-talk key changed to Left Ctrl** (was Right Ctrl) so the right (gesture) hand
  stays free. Also added the mouse **middle button** as a second, independent trigger (holds
  listening active same as the key) for a small wireless mouse kept near the Leap
  controller -- not yet tested with real hardware as of this note.
- **Update 2026-10-04**: push-to-talk key changed again, twice -- Left Ctrl turned out to be
  overloaded elsewhere as a selection-tool modifier key, so briefly tried Space, which turned
  out to be directly bound to the "Isolate And Labels" shortcut (pressing it to talk was
  isolating/labeling the selected part). Settled on **V** -- confirmed unused against every
  keyboard binding in `MainScene.unity`, not just `Shortcuts.cs`. Confirmed working by the
  user; everything else (Whisper ASR backend, recognition accuracy) also confirmed working
  well in this same pass. See `VOICE_AND_GESTURE_COMMANDS.md` for the current key.
- Full user-facing command/gesture list now lives in `VOICE_AND_GESTURE_COMMANDS.md` at
  `docs/` -- keep it in sync with `VoiceCommands.cs`'s vocabulary and the gesture
  scripts if either changes.
- **Voice recognition accuracy on Bluetooth earbuds (2026-07-20)**: root cause of both
  bad transcriptions and unrelated commands swapping for each other turned out to be a
  bug in how this project fed Vosk its constrained-vocabulary grammar (one flat
  alphabetized word list instead of natural phrases), not the earbuds' mic quality --
  see the dated section below for the full fix (grammar rebuilt as a phrase list,
  decode moved off the main thread, a rule-collision guard, and a bigger Vosk model).
- **Leap zoom sensitivity bumped another 3x (2026-07-20)**: `zoomSensitivity` 2000 →
  6000, since the physical two-hand-separation range available on this hardware/desk
  setup is smaller than the full zoom sweep needs.

Hardware: **original Leap Motion Controller** (not LMC2 / not "Ultraleap" hardware),
currently running the legacy **Orion 4.1.0** driver.

## Decision made

Orion 4.1.0 only supports the old, unsupported "Core Assets 4.4.0" Unity plugin, which
predates Unity 2022.3 and isn't maintained. The original LMC is fully supported by
Ultraleap's *current* tracking software though, so we're upgrading the driver instead of
using the legacy plugin. Chose: **upgrade tracking service + use the current, maintained
Ultraleap Unity Plugin (com.ultraleap.tracking).**

## Code changes — done (MediaPipe path untouched, purely additive)

All committed to the working tree (not yet git-committed as of this note):

- `Assets/Scripts/Base/HandInputMode.cs` (new) — `HandInputMode {Off, Video, Leap}` enum
  + `HandInputCoordinator` static mode switch. Defaults to `Video` so behavior is
  unchanged until a new voice command is used.
- `Assets/Scripts/Base/LeapInputProvider.cs` (new) — Leap equivalent of
  `GestureInputProvider`. Reads the first tracked hand's palm position (normalized
  against a tunable interaction volume: `horizontalRange`/`minHeight`/`maxHeight`) for
  mouse-move, and `Hand.PinchStrength` (threshold `pinchThreshold`, default 0.75) for
  pinch. Exposes the same static shape as `GestureInputProvider`:
  `Enabled` / `IsTracking` / `ScreenPosition` / `PinchWasPressedThisFrame` /
  `PinchWasReleasedThisFrame`. Only active while `HandInputCoordinator.Mode == Leap`.
- `Assets/Scripts/Base/GestureInputProvider.cs` — one added condition: `Enabled` now
  also requires `HandInputCoordinator.Mode == Video`. Nothing else changed.
- `Assets/Scripts/Base/PointerInput.cs` — now arbitrates Leap → MediaPipe → mouse
  fallback, in that priority order, based on which backend is enabled+tracking. Added
  `PointerInput.IsHandTracking` helper (true if either backend is active).
- `Assets/Scripts/Base/RaycastObject.cs` — on-screen gesture cursor visibility now uses
  `PointerInput.IsHandTracking` instead of a MediaPipe-only check, so it shows for Leap
  too.
- `Assets/Scripts/Voice/VoiceCommands.cs` — replaced the old `"gestures on"` /
  `"gestures off"` rules with three new ones (see Voice commands below).
- `Packages/manifest.json` — added the Ultraleap OpenUPM scoped registry
  (`https://package.openupm.com`, scope `com.ultraleap`) and dependency
  `com.ultraleap.tracking: 6.15.1`.

## Manual steps remaining (not yet done — pick up here)

1. ~~Upgrade the tracking service on this laptop.~~ Done.
2. ~~Open the project in Unity 2022.3.22f1~~ Done — package resolved.
3. ~~Swap the tracking prefab in the scene~~ Done 2026-07-17 — replaced
   `Service Provider (Screentop)` with
   `Packages/Tracking/Core/Runtime/Prefabs/Service Provider (Desktop).prefab` (controller
   sits flat on the desk).

   Also fixed 2026-07-17: `LeapInputProvider.cs` only had `using Leap;`, but in package
   6.15.1 `LeapServiceProvider`/`LeapProvider` live in `Leap.Unity` (not `Leap` — the API
   reference note below was wrong on this point; `Hand`/`Frame` are correctly in `Leap`).
   Added `using Leap.Unity;` — this was the CS0246 compile error blocking play mode.
4. ~~Add a `LeapInputProvider` component~~ Done 2026-07-17 — added to the `GestureSystem`
   GameObject (same object `GestureInputProvider` lives on), `debugReadoutParent` wired to
   the same `GestureCursor` RectTransform under `#Canvas` that `GestureInputProvider` uses.
5. **Tune in the Inspector while running:**
   - `horizontalRange` / `minHeight` / `maxHeight` (meters) — the physical volume above
     the controller that maps to the full screen. Start with defaults
     (0.15 / 0.10 / 0.40) and adjust while watching the cursor.
   - `pinchThreshold` (0–1 on `PinchStrength`) — start at 0.75, adjust while watching
     the debug readout, same workflow used to tune MediaPipe's `pinchThreshold`.
   - `mirrorX` — flip if the cursor moves opposite to your hand.
6. ~~Test all three voice commands~~ Done 2026-07-17 — "gesture off"/"leap"/"video" all
   confirmed working, cursor pinch-color feedback confirmed in both Leap and Video modes.
   User's take: Leap tracking feels more accurate than the MediaPipe webcam path.

**Leap Motion integration: complete and working end-to-end as of 2026-07-17.**

### Hardware note (2026-07-17)

The original Leap Motion Controller unit + cable this laptop had been using turned out to
be flaky (intermittent USB video-stream drops, garbled device serial number readback in
`C:\ProgramData\Ultraleap\HandTracker\Logs\tracker_log.txt` — `Failed to read value from
address: 0`, `Device serial number changed!`), reproduced on both old (Orion) and new
(Ultraleap) software/across ports — genuine hardware fault, not a driver issue. Swapped to
a different original LMC unit + cable and it tracks fine. Ultraleap Tray warned that one is
in a "low power" USB port; this ASUS laptop only has two USB ports total, so if dropouts
ever recur, get a powered USB hub rather than chasing driver settings again.

### Cursor polish (2026-07-17): pinch-colored cursor, debug text off by default

Added `IsPinching` (continuous, unlike the existing edge-triggered
`PinchWasPressedThisFrame`/`PinchWasReleasedThisFrame`) to both `LeapInputProvider` and
`GestureInputProvider`, exposed through a new `PointerInput.IsPinching`. `RaycastObject`
now tints its existing `gestureCursor` square (the small red square that already follows
the pointer) to `gestureCursorPinchColor` (green by default, public field, tweakable in
Inspector) while pinching, restoring its original color otherwise — backend-agnostic,
works for both Leap and MediaPipe. Replaces relying on the separate on-screen debug text's
green/red coloring for at-a-glance pinch feedback.

`LeapInputProvider`'s `showDebugReadout` default flipped to `false` (MediaPipe's was
already `false` in the scene). **Manual step**: the `LeapInputProvider` component already
in the scene was added before this change, so its serialized `Show Debug Readout` value is
still whatever the Inspector checkbox showed at add-time — uncheck it by hand on the
`GestureSystem` GameObject's `Leap Input Provider` component if it's still on.

### Bug found & fixed (2026-07-17): MediaPipe skeleton overlay ignored the mode switch

`HandSkeletonView.cs` (draws the 21-dot hand skeleton over the webcam-tracked hand, lives
on `GestureSystem`) only checked `showSkeleton`/`_hasHand`/staleness — it never checked
`HandInputCoordinator.Mode`, so it kept rendering (and `GestureSceneLoader` keeps the
MediaPipe scene/webcam pipeline loaded and running) regardless of which mode was active.
Symptom: while in `Leap` mode, moving a hand in front of the laptop webcam (not the Leap
controller) still showed the hand-skeleton overlay. Fixed by adding
`&& HandInputCoordinator.Mode == HandInputMode.Video` to `HandSkeletonView`'s `visible`
check, matching how `GestureInputProvider.Enabled` is already gated. Note:
`GestureSceneLoader` still loads/runs the MediaPipe webcam pipeline continuously
regardless of mode (by design, to avoid reload cost on mode switches) — only the visual
overlay was wrongly always-on.

### Feature added (2026-07-17): gesture-driven UI clicks + full camera manipulation

Two more gaps closed after the base Leap integration was confirmed working:

**1. Pinch didn't click UI elements, only the 3D model.** Root cause: Unity's
`EventSystem`/`InputSystemUIInputModule` (on the `EventSystem` object inside
`Assets/Prefabs/#Canvas.prefab`) only reads the real `Mouse` device for its own pointer
events — a synthetic gesture pinch never reached Buttons etc., only `RaycastObject`'s own
3D-only physics raycast saw it. Fixed with a new `Assets/Scripts/Base/GestureUIInput.cs`:
while a hand-tracking backend is driving the pointer, it raycasts UI at
`PointerInput.Position` via `EventSystem.RaycastAll` (covers every nested Canvas in one
call, no need to wire a specific `GraphicRaycaster`) and manually fires
pointerDown/pointerUp/pointerClick through `ExecuteEvents`. `RaycastObject`'s 3D-raycast
guard was extended to also skip when `GestureUIInput.IsOverUI` is true, so clicking a
button doesn't click through to the body part behind it. Covers `IPointerClickHandler`
(Button/Toggle/Tab) — does **not** yet synthesize drag events, so gesture-driven dragging
of sliders or draggable windows isn't supported, only clicks/taps.
**Manual step**: add the `GestureUIInput` component to any GameObject in the scene (no
fields to wire).

**2. Full two-hand camera manipulation** (`Assets/Scripts/Base/LeapCameraGestures.cs`,
new) — added to make the demo hands-free-complete, replacing mouse-button camera
controls with distinct Leap hand poses chosen specifically not to collide with the
pinch-to-click gesture:
- **Rotate** — one hand, **closed-fist grab** (`Hand.GrabStrength > grabThreshold`,
  default 0.85) + move → same math as right-mouse-drag.
- **Pan** — **two hands** present, moved together → same math as middle-mouse-drag.
- **Zoom** — **two hands** present, distance between them changing (pinch-to-zoom style)
  → same math as scroll-wheel zoom.

Two hands present always takes priority over one-hand grab, so resting a second hand in
view doesn't fight the rotate gesture. All three route through three new small public
methods on `CameraController` (`ApplyGestureRotateDelta`/`ApplyGesturePanDelta`/
`ApplyGestureZoomDelta`) that reuse the exact same `xSpeed`/`ySpeed`/`rotationVelocity`/
`zoomVelocity` tuning constants the mouse path already uses, so gesture and mouse camera
control feel consistent.
**Manual step**: add the `LeapCameraGestures` component to the scene (e.g. alongside
`GestureSystem`/`LeapInputProvider`). Tune while running:
- `grabThreshold` (0–1 on `GrabStrength`) — start at 0.85, lower if fist isn't registering.
- `panSensitivity` (default 1) / `zoomSensitivity` (default 200) — scale factors on top of
  the existing pan/zoom math; raise if movements feel sluggish, lower if too twitchy.
- `horizontalRange`/`minHeight`/`maxHeight` — same interaction-volume shape as
  `LeapInputProvider`'s, kept as separate fields here in case rotate/pan/zoom want a
  different volume than the pointer does.

Not yet tested with real hardware as of this note — pick up here next session.

### Bugs found & fixed (first real-hardware test of click/select + camera gestures)

Two separate defects, both making pinch-to-select look completely dead for both UI and 3D:

1. **`RaycastObject.Update()`'s UI-block check used the real OS mouse position, not the
   gesture cursor.** It gated all 3D highlight/select on `EventSystem.current
   .IsPointerOverGameObject()`, which reflects wherever the physical mouse was last left —
   completely decoupled from the Leap-driven cursor. Since the mouse isn't touched at all
   during hands-free use, it sits wherever it last was; if that happens to rest over any UI
   element anywhere on screen, this returned `true` unconditionally and blocked 3D
   selection regardless of where the hand/gesture cursor actually pointed. Fixed: now uses
   `GestureUIInput.IsOverUI` (gesture-cursor-based) instead of the real-mouse check whenever
   `PointerInput.IsHandTracking` is true.
2. **`LeapInputProvider` invalidated tracking (and silently dropped any in-progress pinch)
   on any single frame with zero reported hands**, rather than tolerating brief gaps the
   way `GestureInputProvider`'s MediaPipe path already does. Leap can miss a hand for one
   stray frame even with healthy tracking (motion blur, brief occlusion) — and given this
   hardware's well-documented flakiness (see hardware notes above), that's common enough
   that a full press+release pinch rarely survived intact. Worse, the old code reset
   `_wasPinching = false` on that empty frame *without* firing
   `PinchWasReleasedThisFrame`, so the pending click-down was silently swallowed rather than
   ever completing. Fixed: `LeapInputProvider` now caches the last known position/pinch
   state and only invalidates after `trackingTimeout` (0.3s) of *no* hand at all, mirroring
   `GestureInputProvider`'s already-correct behavior; and if a pinch really was in progress
   when tracking is genuinely lost, it now fires a release to close the gesture out cleanly
   instead of leaving state stuck.

**Not yet verified against real hardware** — pick up here: retest pinch-select on both 3D
body parts and UI buttons.

### Zoom gesture redesigned: two-hand pinch instead of raw two-hand spread (same session)

The original zoom (raw hand-separation change, applied simultaneously with pan from raw
midpoint movement, whenever two hands were present) turned out to be hard to trigger
cleanly — an imperfect/diagonal "pull apart" motion shifts the midpoint enough that pan
dominates and swallows the zoom. Replaced with the user's suggested double-pinch design:
**both hands must be pinching at once to engage zoom**; while both stay pinched, the
change in separation between them drives zoom amount (spread apart to zoom in, bring
together to zoom out). Two hands present *without* both pinching still pans as before —
this cleanly disambiguates the two gestures instead of trying to blend them. Since zoom
now reuses the same thumb-index pinch as one-hand click, `LeapCameraGestures` exposes
`TwoHandPinchZoomActive` (true while both hands pinch) and `LeapInputProvider` suppresses
hand[0]'s own click while it's true, so a two-hand pinch-zoom can't also fire a spurious
click wherever the pointer happens to be.

**Not yet verified against real hardware** — pick up here next session.

### Tracking lost on Unity stop/re-play, recovered by toggling device orientation (open, unexplained)

User observed: stopping and re-entering Play mode sometimes left tracking dead — no hands
in Unity *and* no hands in the standalone Ultraleap tracking software/visualizer either.
Toggling the device-orientation setting in the Ultraleap software from inverted back to
normal (or vice versa) brought it back.

Checked whether anything in this project could be responsible: neither
`Assets/Resources/Ultraleap Settings.asset` (the package's global settings asset) nor the
`LeapServiceProvider` component in the scene has any orientation-related field, and Unity
has no access to the Ultraleap tracking *service's* own device-orientation config (that
lives outside the project, managed by the Ultraleap Control Panel / tray app) — so this is
not something the app or this session's code changes are doing. Since tracking was dead in
the *standalone* Ultraleap software too (not just Unity), this points at the same class of
hardware/service flakiness already logged above, not a Unity-side regression. Best guess:
flipping the orientation setting incidentally forces the tracking service to
reinitialize/re-negotiate its connection to the device (a "kick," same effect as the
USB-port-swap fixes noted earlier), rather than orientation itself being the actual fix.

**Not concluded — user is re-testing.** Next time it happens, check
`C:\ProgramData\Ultraleap\HandTracker\Logs\tracker_log.txt` (the live-monitoring one-liner
above) for the same `Broken device`/`Failed to read`/serial-number-flip signature already
seen with this hardware. If present, it confirms this is the same recurring connection
issue and the orientation toggle is just an incidental service restart, not a real fix.

**New detail (2026-07-17, later)**: the fix is asymmetric. Flipping the orientation
setting to its *other* value wakes tracking back up, but immediately flipping it back to
the original value afterward does **not** re-break it -- tracking stays fine. So this isn't
"being in inverted mode specifically fixes it" or a simple toggle-state effect; whichever
direction you flip, tracking recovers and then holds. That's more consistent with *any*
settings change forcing the Ultraleap tracking service to reinitialize/reconnect to the
device (a generic "kick," the same underlying effect as the USB-port-swap fixes earlier in
this file) than with orientation being functionally meaningful. Still unresolved -- if a
faster/less disruptive way to force that same reconnect exists (e.g. a "restart tracking
service" option in the Ultraleap Control Panel, or toggling some other unrelated setting),
it would probably work just as well as the orientation flip and confirm this theory.

### Round 2: debug readout was invisible, zoom gesture swapped again (same investigation)

**Debug readout stayed invisible even after reparenting to the Canvas.** Root cause: the
reparent code found the Canvas via `debugReadoutParent.GetComponentInParent<Canvas>()` —
but `GetComponentInParent` only walks *active* GameObjects by default, and
`debugReadoutParent` (the `GestureCursor` object) is frequently inactive at the exact
moment this first runs (before tracking has started), so the lookup silently failed and
fell through to the `??` fallback — which was `debugReadoutParent` itself, i.e. the exact
broken parenting being fixed. Since the text object is only ever built once and cached,
this was permanent for the whole Play session. Fixed by passing `includeInactive: true`.
**Not yet confirmed visible on hardware as of this note** — pick up here.

**Zoom gesture reverted from double-pinch back to raw two-hand separation.** Real-hardware
testing of the double-pinch design (both hands pinch, then pull apart/together) showed it
barely zoomed and was direction-asymmetric (reliably zoomed out, rarely zoomed in). Cause:
holding a strong pinch through a full arm-spread is hard on this hardware — pinch strength
tends to relax as the arm extends, so the "spread apart" (zoom-in) direction drops out of
the gesture (loses `bothPinching`) before much separation change accumulates, while "bring
together" (zoom-out) stays comfortably pinched throughout. Reworked back to raw
hand-separation (no pinch required, per the original design), but this time properly
disambiguated from pan instead of applying both simultaneously: each frame, compares
midpoint movement (world meters) against separation change (world meters) — same units, so
directly comparable — and only applies whichever is larger. `zoomSensitivity` also bumped
10x (200 → 2000) per user request since even a working zoom read as barely perceptible.
`LeapCameraGestures.TwoHandPinchZoomActive` renamed to `TwoHandGestureActive`, now true for
any two-hand gesture (pan or zoom) rather than only pinch-zoom, so `LeapInputProvider`
suppresses hand[0]'s click any time two hands are present, not just during a pinch-hold.
**Not yet re-verified on hardware as of this note.**

**Selection: root cause found via the (now working) debug readout.** Confirmed 2026-07-17:
readout showed `Leap: 0.88 PINCHING`, `UI:false`, `obj=hip bone` (the 3D raycast correctly
found the target), but `release dist=300px` -- pinch detection and hit-testing were both
fine, the click was being rejected purely by `RaycastObject.Select()`'s press-to-release
distance check, which used a flat 10px tolerance clearly tuned for a mouse. A pinch-hold
naturally drifts far more than that (small hand movement maps to large screen movement
given the small interaction volume), so real clicks were failing outright. Fixed: added
`RaycastObject.handTrackingClickTolerance` (default 150px), used instead of the 10px mouse
tolerance whenever `PointerInput.IsHandTracking` is true. Nothing else depends on the mouse
value being tight -- `BoxSelection` reads the real `Mouse` device directly, not
`PointerInput`. **Not yet re-verified on hardware as of this note** -- pick up here: retest
a UI button and a 3D body part; if `release dist=...` is still over 150 on a real click
attempt, raise `handTrackingClickTolerance` further in the Inspector.

**Pan and zoom confirmed working well** (2026-07-17, after the raw-separation
zoom/disambiguation rework above).

### Selection round 2: distance-tolerance tuning was the wrong approach entirely

The 150px `handTrackingClickTolerance` fix above wasn't enough -- live testing showed ~2/20
hit rate, numbers "all over the place" (release dist values well past 150px on slow,
deliberate pinches; fast pinches landed more often). That pattern says the real problem
isn't the tolerance *value*, it's comparing press-position to release-position at all for a
hand that's going to keep drifting the longer it holds a pinch -- no fixed threshold could
be both tight enough to feel intentional and loose enough to survive a real hold.

Fixed properly: `RaycastObject.Select()` now snapshots `objectSelected`/`bodyPartScript`/
`hit` at the moment of *press* (new `pressedObject`/`pressedBodyPart`/`pressedHit` fields)
and, for hand-tracking input only, acts on that press-time snapshot at release instead of
re-checking the live (release-time) target -- and skips the distance gate entirely, since
there's no drag gesture routed through `PointerInput` it would need to disambiguate against
(`BoxSelection` reads the real `Mouse` device directly, not `PointerInput`). Mouse clicks
are untouched: still release-time target, still a tight 10px tolerance. `handTrackingClickTolerance`
removed (no longer used).

**Not yet re-verified on hardware as of this note** -- pick up here: retest a UI button and
a 3D body part.

### Selection round 3: pinch signal itself was chattering (hysteresis added)

Even with the press-time-snapshot fix above, live hit rate was still ~1-in-10-to-20, and
notably *worse* the longer/slower the pinch hold -- fast pinches landed more often. That
pattern points upstream of the click logic entirely: `hand.PinchStrength` is a noisy analog
value that can dip below and back above a single shared threshold (0.75) during a real
hold, which reads as a spurious release-then-press pair splitting one intended pinch into
several fragments (each snapshotting whatever was under the cursor at that instant, not
necessarily the real target) -- more exposure time during a slow hold, more chances to
chatter.

Fixed with hysteresis: `LeapInputProvider.pinchThreshold` split into `pinchOnThreshold`
(0.75, needed to *start* a pinch) and `pinchOffThreshold` (0.5, needed to *end* one, once
already pinching) -- a dead-band the noisy signal can't chatter back and forth across mid-hold.
**Not yet re-verified on hardware as of this note.** If hit rate is still low after this,
next thing to try: watch the debug readout's PINCHING/not-pinching text during a single
slow, deliberate pinch-hold -- if it still flickers between the two, add a short frame-count
debounce on top of the hysteresis (e.g. require 2-3 consecutive frames before committing to
a state change) rather than widening the hysteresis band further.

### Selection round 4: added `hands:`/`2Hgesture:`/`since fresh hand:` diagnostics, ruled
out tracking dropout and phantom-second-hand, found the real cause: chatter + toggle click

Live testing with hysteresis in place still showed ~1-in-10-to-20 hit rate, worse on slow
holds, better on fast pinches -- same pattern as before hysteresis, just less extreme.
Added diagnostics to `LeapInputProvider`'s debug readout to test two remaining theories:
`hands:` (raw Leap hand count that frame, tests whether a phantom second hand is
triggering `LeapCameraGestures.TwoHandGestureActive` and suppressing the one-hand pinch)
and `since fresh hand:` (ms since Leap last actually reported a hand, tests whether
tracking is only intermittent and the grace-window cache is masking dropouts). Live result:
`since fresh hand` stayed at 0 throughout (tracking genuinely fresh every frame) and
`hands:` stayed correctly at 1 (no phantom hand) -- both ruled out.

That redirected attention to the click *action* itself: `TangibleBodyPart.ObjectClicked()`
(`Assets/Scripts/Base/TangibleBodyPart.cs:74`) is a select/deselect **toggle**. If a slow
held pinch generates an even number of spurious press/release pairs internally, the net
effect is select-then-immediately-deselect -- invisible, indistinguishable from "nothing
happened," even though clicks did fire correctly. Given tracking is confirmed fresh, the
chatter must be in the raw `PinchStrength` signal itself during a sustained hold (real
muscle tremor or algorithmic noise, not a tracking gap) -- meaning magnitude-based
hysteresis alone wasn't a wide-enough net if the real signal dips below `pinchOffThreshold`
and back mid-hold.

Fixed: added a time-based debounce on top of the hysteresis (`releaseDebounceSeconds`,
default 0.15s) -- once pinching, a frame reading below `pinchOffThreshold` no longer
immediately commits to a release; it only counts as released once the signal has stayed
below that line continuously for the debounce window. A brief dip that recovers within the
window is treated as still one continuous pinch.

### Selection round 5: THE actual root cause -- gesture cursor was raycasting into itself

After the debounce fix, live testing got *worse*, not better -- zero hits, and critically
the readout showed `UI:true` continuously even with the cursor held over a close-up 3D
bone, nowhere near any real UI. That's the real smoking gun and explains every symptom
observed across all previous rounds in one shot.

Root cause: the gesture cursor (`RaycastObject.gestureCursor`, the small square that
visually follows the pointer and tints green/red on pinch) had `Raycast Target` enabled on
its `Image` component -- Unity UI's default. `RaycastObject.LateUpdate()` repositions that
cursor to `PointerInput.Position` every single frame -- the *exact same point*
`GestureUIInput.Update()` raycasts UI at. The cursor was raycasting into itself and
reporting "yes, there's UI here" regardless of what was actually behind it. Since
`RaycastObject.Update()`'s guard (fixed in round 1) treats `GestureUIInput.IsOverUI` as
authoritative while hand-tracking, this permanently blocked `Highlight()`/`Select()` from
ever running for 3D targets, and separately fed `GestureUIInput` its own icon
(`Results[0]`) instead of whatever real button was underneath, breaking UI clicks too.

This also explains the "fast pinches work better" pattern that showed up in every round of
testing: the cursor is repositioned in `LateUpdate`, which runs *after* `GestureUIInput`'s
`Update` already raycasted that frame -- a one-frame lag between the cursor's rendered
position and the raycast point. Fast hand movement makes that lag large enough to miss;
slow, careful aiming (exactly what selecting a specific bone requires) keeps the cursor
almost exactly where it was raycasted, landing the self-hit almost every time.

Fixed: `gestureCursorImage.raycastTarget` explicitly forced to `false` in
`RaycastObject.LateUpdate()` right where the Image is first cached, plus the same field
corrected directly in the scene's serialized `GestureCursor` Image component (was
`m_RaycastTarget: 1`, now `0`) so it's correct from frame one, not just after the cache
runs. The cursor is a purely visual indicator and must never be a raycast target itself.

This is very likely the actual root cause behind the whole multi-round selection saga --
all the other fixes this session (EventSystem gate, tracking-dropout release-swallowing,
press-time snapshot instead of distance gate, pinch hysteresis + debounce) were real bugs
worth fixing in their own right, but this one alone could have been masking whether any of
them were actually working, since 3D selection was unconditionally blocked the entire time
regardless of what the pinch signal was doing. **Not yet re-verified on hardware as of this
note -- highest-confidence fix of the whole investigation, pick up here first.**

### Recurring hardware instability, 3rd unit swap (2026-07-17, later same day)

After a reboot, tracking regressed again: Ultraleap Visualizer showed hands with no
skeleton, or a skeleton that appeared briefly then vanished. `tracker_log.txt`
(`C:\ProgramData\Ultraleap\HandTracker\Logs\`) showed the same failure signature as the
original hardware note above — repeating `Broken device detected, missing frame data`,
`tracking loop exception: Failed to read value from address: N`, and the device's reported
serial number flipping mid-session (`LP03442818095` → `LP96384335430`, same physical
unit — a garbled USB read, not an actual device change).

**Diagnosis steps that worked**: traced the actual `Leap Motion Controller` PnP device's
parent chain (`Get-PnpDevice` + `Get-PnpDeviceProperty -KeyName DEVPKEY_Device_Parent`)
instead of guessing from the flat Device Manager list — this laptop (ASUS ROG Zephyrus G14
GA402X, AMD/Ryzen, **no Thunderbolt** — the bolt-icon USB-C port is USB4, not Thunderbolt)
shows 4x "AMD USB 3.10 eXtensible Host Controller" + 4x "USB Root Hub (USB 3.0)" entries
that are otherwise indistinguishable by name. Both physical USB-A ports on this machine
route through the *same* internal xHCI controller, so switching ports doesn't change the
PCI host controller — it does change the USB port-path suffix in the instance ID
(`&0&0000` → `&2&0000`) and forces a full device re-enumeration, which is the useful signal
to watch for instead.

**Port guidance for this device**: use the plain USB-A 3.2 Gen2 ports directly, no adapter.
Do not use the USB4 (bolt-icon) port even with a USB-C→USB-A adapter — USB4 tunnels legacy
USB traffic through an extra translation layer (known weak spot for simple bus-powered
devices), and an adapter adds another physical contact point, both working against a device
that's already showing signal-integrity symptoms.

**What actually fixed it this time**: switched the cable to the laptop's other USB-A port →
errors stopped for a while, but tracking quality was still poor (`hand_created`/`hand_lost`
counters both fired for both hands shortly after appearing — hands detected then dropped,
no USB errors during that window, so a genuinely different symptom from the connection
drops: pointed at lighting/dust/hand-range rather than USB). Then swapped in a **third**
physical LMC unit (previous two: the original, and the one swapped in per the hardware note
above) — tracking became stable and accurate immediately, no further errors.

**Live-monitoring one-liner** (prints only error/disconnect lines from recent log tail, use
to self-check without pulling the whole log):
```powershell
Get-Content "C:\ProgramData\Ultraleap\HandTracker\Logs\tracker_log.txt" -Tail 500 | Select-String "Broken device|Failed to read|serial number changed|OnDeviceChange"
```

**Open concern, not yet resolved**: three physical LMC units in a row have each started
clean and degraded later. This may mean the units (all ~10+ years old) are failing at
different rates independent of each other, or there's a shared cause (port power budget,
cable, or environmental) that outlasts any single swap. Treat "it's tracking well right
now" as provisional — watch for recurrence rather than assuming this is permanently fixed.
If it recurs on this (3rd) unit too, next things to try: a self-powered (external
wall-adapter) USB 3.0 hub to rule out laptop USB port power-budget issues entirely, and
checking for direct sunlight/bright incandescent light near the sensor (classic Gen1 Leap
tracking-quality killer, separate from the connection-drop symptom).

### Voice recognition accuracy overhaul + Leap zoom tuning (2026-07-20)

**Starting complaint**: user switched from a wired USB headset (worked well) to Soundcore
P3i true-wireless Bluetooth earbuds for a fully cordless hands-free setup (no cable fighting
the Leap gestures), but recognition got noticeably worse -- e.g. "reset view" heard as "he
said few". Isolated the cause by testing the same phrases via Windows dictation on the same
earbuds: all recognized correctly, proving the mic/Bluetooth audio path itself was fine and
the weak link was `VoiceCommands.cs`'s bundled `vosk-model-small-en-us-0.15` model (a ~40MB
compact model, meaningfully less accurate than Windows' own speech engine at any vocabulary
size).

Fixes applied, in order:

1. **Grammar-constrained Vosk recognition.** Instead of open-vocabulary decoding,
   `VoskRecognizer` is now constructed with a JSON grammar restricting it to only the
   words/phrases this app's voice commands actually use (`grammarWords`/`grammarPhrases`,
   built via `BuildGrammarJson()`) plus Vosk's `"[unk]"` catch-all for filler words. Every
   word a rule can trigger on is captured automatically as rules are added in
   `BuildVocabulary()` (via `Rule()`/`ParamRule()` → `RegisterGrammarWords()`) -- adding a
   new voice command the normal way needs no separate grammar list to maintain.
2. **Bigger model**: swapped in `vosk-model-en-us-0.22-lgraph` (~130MB, downloaded from
   alphacephei.com) in place of the small model -- both `VoiceCommands.modelFolderName`'s
   C# default and the value already serialized on the component in `MainScene.unity` were
   updated (Unity serializes the Inspector value separately from the code default). Chose
   the mid-size `-lgraph` variant over the full ~1.8GB model as a lighter first step; full
   model is the next lever if needed. User's hardware (RTX 4080 + Ryzen 7000-series laptop)
   has ample headroom for either.
3. **Vosk decode moved off Unity's main thread.** `VoiceProcessor` was invoking
   `recognizer.AcceptWaveform`/`PartialResult`/`FinalResult`/`Reset` synchronously from a
   main-thread coroutine -- fine for the tiny model, but a bigger model's higher per-chunk
   decode cost risked frame hitches during push-to-talk. Added a dedicated background
   `Thread` (`decodeThread`) that owns the recognizer exclusively; audio frames and
   finalize requests are handed off via a `BlockingCollection<DecodeCommand> decodeQueue`
   processed strictly in FIFO order (relied on so e.g. a new utterance's `Reset` always
   runs after any leftover frames from the previous one). `FinishListening`'s coroutine
   polls a small `volatile`-flagged `DecodeResult` via `WaitUntil` instead of blocking.
4. **Rule-collision guard.** Found (separately from the accuracy work) that "slice in 40
   percent" sometimes fired **"remove slice"** instead -- the reset-ladder rules match by
   bag-of-words keyword presence with no ordering/adjacency requirement, so a single
   mis-heard word landing on "reset"/"remove" (grammar-constrained decoding forces
   ambiguous audio onto the *closest available* grammar word) could satisfy an unrelated
   rule. Added `VoiceCommandRule.SkipIfNumberHeard`, set on the whole reset ladder
   ("remove slice"/"clear"/"reset view"/"reset"/"whole body"/"start over") -- these never
   legitimately co-occur with a spoken number, so hearing one skips straight to checking
   other rules instead of firing.
5. **The actual root cause of cross-command confusion (e.g. "slice"/"rotate" swapping,
   "slow rotate" always losing to "rotate by X degrees"), found after the above didn't
   fully fix it even on the *wired* headset**: `BuildGrammarJson()` had been feeding Vosk
   its entire vocabulary as **one flat, alphabetically-sorted string** (e.g. "...rotate
   ... slice ... spin..."). Vosk's grammar mode uses the given word list to bias which
   words the decoder expects to follow which -- alphabetical adjacency isn't real phrase
   structure, so ambiguous audio got biased toward nonsense transitions instead of how
   commands are actually spoken, explaining both the illogical swaps and "slow rotate"
   hallucinating a number (which made the numeric `rotate by degrees` `ParamRule`, checked
   first, win). Fixed by rebuilding the grammar as a **list of natural phrases** instead:
   - `AddLayerRule`/`AddHeroOrganRule`/"remove slice"'s word-group order was swapped to
     read verb-first (e.g. `[["show","add"], subjectWords]` instead of
     `[subjectWords, ["show","add"]]`) -- matching logic (`RequiredWordGroups.All(...)`)
     is order-independent so this is purely cosmetic for phrase generation, not a
     behavior change.
   - `AddPhraseCombinations()` (called from `RegisterGrammarWords()`) auto-generates every
     natural phrase combination from a rule's word groups in that order (e.g. "show
     muscle", "show muscles", "add muscle", "add muscles") into `grammarPhrases`.
   - `ParamRule()` gained an optional `examplePhrases` param -- since a `ParamRule`'s word
     groups alone don't include where the number goes or surrounding filler ("percent"/
     "degrees"), each of the 5 ParamRules (`zoom in/out by percent`, `rotate by degrees`,
     `slice to percent`, `slice deeper by percent`) now also seeds a few explicit example
     phrases with real numbers (e.g. "rotate by twenty degrees", "slice at forty percent")
     to establish that numeric slot's context. Individual number words are still added as
     standalone one-word phrase entries too, so any spoken number can fill those slots.
   - `BuildGrammarJson()` now joins `grammarPhrases` as separate JSON array entries
     (`["show muscle", "show muscles", ..., "[unk]"]`) instead of one big string.

**Confirmed by user as "works well enough" after fix #5** on both the earbuds and the wired
headset. Not re-tested since: whether the full ~1.8GB model would improve further, or
whether other rule pairs beyond slice/rotate have similar latent bag-of-words collisions
`SkipIfNumberHeard`-style guards haven't covered.

**Windows-side mic quality tips also given (not implemented, informational only)**: install
NVIDIA Broadcast (RTX-accelerated noise removal virtual mic -- biggest lever given this is
an RTX 4080 laptop), check for a Bluetooth adapter driver update to enable wideband HFP
(mSBC 16kHz) instead of narrowband (CVSD 8kHz), tune Recording-device levels/enhancements in
the Windows Sound control panel, and check for Windows 11 LE Audio support (Bluetooth 5.2+,
LC3 codec) as a longer-shot upgrade over classic HFP.

**Leap zoom sensitivity**: `LeapCameraGestures.zoomSensitivity` raised another 3x (2000 →
6000, both the C# default and the value serialized on the scene's component) -- physical
two-hand-separation range available on this desk/hardware setup runs out before covering a
full zoom sweep, so the same range of motion now covers roughly 3x more zoom distance.
**Not yet tested against real hardware as of this note.**

## How the hand gestures work (user-facing reference)

Everything below applies in `Leap` mode (see Voice commands). The same pinch-to-click
gesture also works in `Video`/MediaPipe mode with its own thresholds noted inline.

**Pointer / cursor movement** — move one hand above the controller. Its palm position is
read relative to the controller's own flat-mounted axes and mapped to the screen using a
tunable interaction volume (`LeapInputProvider`: `horizontalRange`/`minHeight`/`maxHeight`,
default 0.15m / 0.10m / 0.40m above the sensor). The small red square cursor
(`RaycastObject.gestureCursor`) follows this position and turns **green**
(`gestureCursorPinchColor`) while pinching, red otherwise — this is the primary at-a-glance
feedback, replacing the old on-screen debug text (which is off by default,
`showDebugReadout`).

**Pinch = click/select** — bring thumb and index finger together. Leap mode uses the
controller's own `Hand.PinchStrength` (0–1), threshold 0.75 (`LeapInputProvider.pinchThreshold`).
MediaPipe/Video mode instead measures raw thumb-index tip distance, threshold 0.035
(`GestureInputProvider.pinchThreshold`) — different units, same concept. Pinching:
- Over the 3D anatomy model → selects/interacts via `RaycastObject`'s physics raycast.
- Over a UI element (Button/Toggle/Tab) → clicks it via `GestureUIInput`, which
  synthesizes real `IPointerClickHandler` events so gesture pinches reach Unity UI, not
  just 3D objects. **Not supported yet**: dragging sliders or draggable windows by
  pinch — only discrete clicks/taps.

**Camera control (Leap mode only, `LeapCameraGestures`)** — three distinct hand poses,
chosen specifically so none of them collide with the thumb-index pinch used for clicking:
- **Rotate**: one hand, **closed fist** (`Hand.GrabStrength > 0.85`, `grabThreshold`) +
  move → same math as right-mouse-drag.
- **Pan**: **two hands** present, **not** both pinching, moved together → same math as
  middle-mouse-drag. Sensitivity: `panSensitivity` (default 1).
- **Zoom**: **two hands, both pinching** (bring index+thumb together on each hand at once,
  like a double pinch/grab), then pull apart to zoom in or bring together to zoom out while
  staying pinched → same math as scroll-wheel zoom. Sensitivity: `zoomSensitivity` (default
  200), pinch threshold `pinchThreshold` (default 0.75, separate field from
  `LeapInputProvider`'s own). Releasing either hand's pinch ends the zoom gesture and drops
  back to one-hand click / pan as normal.

Two hands present always overrides one-hand grab-rotate, so resting a second hand in view
doesn't fight the rotate gesture. Rotate/pan/zoom route through `CameraController`'s
existing `ApplyGestureRotateDelta`/`ApplyGesturePanDelta`/`ApplyGestureZoomDelta` methods,
which reuse the same tuning constants the mouse-drag camera controls already use, so
gesture and mouse camera control feel consistent with each other.

## Voice commands (new vocabulary, replaces "gestures on/off")

- **"gesture off"** (or "disable") → `HandInputMode.Off` — hand tracking fully off,
  falls back to mouse.
- **"gesture leap"** → `HandInputMode.Leap` — Leap Motion controller drives the pointer.
- **"gesture video"** (or "camera") → `HandInputMode.Video` — MediaPipe webcam pipeline
  drives the pointer (this is the default on startup).

## Key API references used (for LeapInputProvider, in case it needs revisiting)

- Package: `com.ultraleap.tracking` 6.15.1 via OpenUPM, min Unity 2021.3.
- `Leap.Unity.LeapServiceProvider : Leap.Unity.LeapProvider`, `abstract Frame CurrentFrame { get; }`
  (corrected 2026-07-17 — these two are in `Leap.Unity`, not `Leap`).
- `Leap.Frame.Hands` — `List<Hand>`.
- `Leap.Hand.PalmPosition` (Vector3, Unity world space, meters, already transformed by
  the provider's own GameObject transform), `Hand.PinchStrength` (float 0–1).
- No `InteractionBox` class in this plugin version (that was legacy Core Assets API) —
  hence the manual `Mathf.InverseLerp` normalization against the provider's local
  transform in `LeapInputProvider`.
- "Service Provider (Desktop)" prefab ships at
  `Packages/Tracking/Core/Runtime/Prefabs/Service Provider (Desktop).prefab`, identity
  transform, so its local axes match world axes when placed unrotated.
