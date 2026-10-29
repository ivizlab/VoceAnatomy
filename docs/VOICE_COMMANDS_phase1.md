# Voice Commands — Phase 1 (Milestone 2)

> **Superseded** — this is the original Phase 1 snapshot (Vosk-only, Right Ctrl trigger).
> For the current push-to-talk key, ASR backend selection, and full command list, see
> `docs/VOICE_AND_GESTURE_COMMANDS.md` instead.

Push-to-talk voice control for the presenter demo, built on **Vosk** (offline speech recognition — no cloud, no API key, same behavior on any machine regardless of Windows speech settings).

## How it works

1. Hold **Right Ctrl** → mic starts recording, on-screen "Listening…" indicator shows.
2. Release → indicator hides, but the mic keeps capturing for a brief trailing window (`VoiceCommands.trailingCaptureSeconds`, default 0.6s, tunable in the Inspector) before actually stopping and transcribing — this prevents the last word of a phrase from being cut off if you release right as you finish speaking. Vosk then transcribes what was captured, and the transcript is matched against a fixed vocabulary of phrases.
3. A match calls the app's own existing functions (show/hide layers, isolate/reset, camera framing) — voice never touches scene objects directly.
4. No match → logged as "No command matched," app keeps running normally.

This is **keyword matching, not natural-language understanding**. Vosk only converts audio to text; a separate rule table then checks the transcript for required words, using fuzzy (edit-distance) matching so small mis-transcriptions (e.g. "move" heard instead of "remove") still match without needing every near-miss spelling listed by hand — see "How fuzzy matching works" below. Adding a real LLM intent layer on top of this is possible later but was intentionally deferred — see the original phase plan.

## Full command list

| Say | Does |
|---|---|
| "reset" / "whole body" / "start over" | Resets to the default clean state (safety net, always works) |
| "show muscles" / "add muscles" | Shows the muscular layer |
| "remove muscles" / "hide muscles" | Hides the muscular layer |
| "show skeleton" / "show bones" | Shows the skeletal layer |
| "remove skeleton" / "remove bones" | Hides the skeletal layer |
| "show nerves" | Shows the nervous layer |
| "remove nerves" | Hides the nervous layer |
| "show organs" | Shows the visceral/organ layer |
| "remove organs" | Hides the visceral/organ layer |
| "show blood vessels" / "show arteries" / "show veins" | Shows arteries + veins |
| "remove blood vessels" / "remove arteries" / "remove veins" | Hides arteries + veins |
| "show heart" | Reveals the heart structure specifically |
| "show brain" | Reveals the brain structure specifically |
| "show lungs" | Reveals the lungs structure specifically |
| "show liver" | Reveals the liver structure specifically |
| "show stomach" | Reveals the stomach structure specifically |
| "show lymph" / "show lymphatics" | Shows the lymphoid layer |
| "remove lymph" / "remove lymphatics" | Hides the lymphoid layer |
| "show joints" | Shows the joints layer |
| "remove joints" | Hides the joints layer |
| "focus" / "zoom in" | Zooms/frames the camera on whatever was last shown (e.g. after "show heart") |
| "front" / "face front" | Resets camera orientation to the default front-facing view — keeps current zoom |
| "pan up" | Pans the camera up by a fixed 15% of the current view distance |
| "pan down" | Pans the camera down by a fixed 15% of the current view distance |
| "zoom out" | Reframes the camera on everything currently visible |
| "zoom in <N> percent" | Moves N% closer to the current view (e.g. "zoom in ten percent" is a smaller step than "zoom in twenty percent") |
| "zoom out <N> percent" | Moves N% farther from the current view |
| "rotate" / "turn" / "spin" | One full 360° camera spin around the current focus point, ~2 seconds, ends back at start |
| "slow rotate" / "slow turn" / "slow spin" | Same, ~4 seconds |
| "rotate by <N> degrees" ("turn"/"spin" also work) | Rotates by exactly N degrees around the current focus point and stops there (does not return to start), at the same angular speed as the full-spin commands |
| "slice" / "cross section" | Turns on the front-to-back cross-section at the front (0% — nothing removed yet) |
| "slice <N>" | Steps the cross-section plane N% deeper (front → back), relative to wherever it currently is. Repeatable — keep saying it to go deeper. Auto-activates the slice if it's off. |
| "slice back" | Steps the plane a fixed 10% back toward the front. Auto-activates if off. |
| "slice at <N>" | Jumps the plane to an absolute N% position (0% = front, 100% = back). Auto-activates if off. |
| "slow slice" | Auto-sweeps the plane front → back → front (7 seconds each way) and settles at the front on its own — no stop command needed |
| "remove slice" / "slice reset" | Turns the slice off, restores the full solid body. Does **not** touch layer visibility. |
| "clear" / "clear effects" | Cancels the slice + resets camera orientation to front-facing. **Keeps** current zoom. |
| "reset view" / "reset camera" | Resets camera to default framing & zoom (orientation + distance). **Keeps** current layers/effects. |

`<N>` can be spoken as a number word ("ten", "twenty five", "one hundred") — Vosk's small model doesn't transcribe digits from speech, so number words are what get matched. If the same trigger words ("zoom in"/"rotate"/"slice") are heard with no number, the plain non-numbered command above still fires as a fallback.

### Reset ladder

Three deliberately distinct tiers, gentle → nuclear, so no single word means four different things:
1. **"clear"** — slice off + orientation reset, keeps zoom.
2. **"reset view" / "reset camera"** — camera fully reset (zoom + orientation), keeps layers.
3. **"reset" / "whole body" / "start over" / "reset everything"** — full nuclear reset: default camera, all effects off, back to skeleton only.

Internally, the more specific phrases ("remove slice", "clear", "reset view") are checked *before* the bare "reset" safety net, since "reset view"/"reset everything"/"slice reset" all contain the word "reset" too and would otherwise be swallowed by it. See the "Reset ladder" comment block at the top of `BuildVocabulary()`.

### Compound commands

One utterance can chain multiple commands by saying "and" or "then" between them, e.g. "show the heart and focus on it" or "zoom in twenty percent and rotate". Each half is matched and executed as an independent command, ~400ms apart. Capped at 3 chained commands — anything beyond that in one utterance is ignored.

Vocabulary lives in `Assets/Scripts/Voice/VoiceCommands.cs` → `BuildVocabulary()`. Word groups are AND'ed together, words within a group are OR'd — e.g. "show muscles" fires on any transcript containing ("muscle" OR "muscles") AND ("show" OR "add").

### Not voice-controlled this round

- **Skin** — real in the app (a separate progressive dissection-depth slider, not a simple on/off layer like the others) but intentionally left out of this round.
- **Hero structures dropped**: skull, spine/vertebral column, ribcage, pelvis, kidneys, femur — none resolve to a single object. Skull/spine/ribcage/pelvis don't exist as a named group at all (made of many separate bones); kidneys and femur exist only as separate left/right objects with no combined node.

## How fuzzy matching works

Each heard word is checked against each required vocabulary word with a length-scaled Levenshtein (edit) distance, in `WordGroupContains`/`LevenshteinDistance`:
- Words of 3 letters or fewer (e.g. "in", "out", "add") require an **exact** match — fuzzy-matching very short common words causes false positives (e.g. "add" vs "and").
- Words of 4–5 letters allow edit distance ≤ 1.
- Words of 6+ letters allow edit distance ≤ 2.

This catches close mis-transcriptions (e.g. "move" ~ "remove", distance 2) without needing every near-miss spelling hand-listed, and without a network call — still fully offline. It's still keyword-level, not phonetic; a mis-hear that isn't textually close to the target word (e.g. a completely different homophone) won't match.

## Known limitations / next steps

- **Rigid exact-word matching for phrasing (not spelling).** Casual phrasing that doesn't contain the trigger words won't match (e.g. "let's take a look at what's underneath" won't trigger "show muscles") — fuzzy matching only helps with mis-*transcribed* words, not different word choices. A real fix is an LLM intent layer between transcript and command — deliberately not built yet (see original phase plan's "Conversational" phase).
- Small Vosk model (`vosk-model-small-en-us-0.15`) trades accuracy for size/speed; a larger Vosk model would recognize more reliably but takes longer to load and more disk space.

## Setup notes for a new machine (e.g. laptop)

Everything below is already committed/saved in this repo once you commit — a fresh `git clone` + open in Unity should "just work," but double check:

- **Unity 2021.3.45f2**, same as desktop. Voice does not need the 2022.3 upgrade.
- **Windows microphone permissions**: Settings → Privacy & security → Microphone → both "apps" and **"Let desktop apps access your microphone"** must be on (the second one is easy to miss and is what the Unity Editor/build needs).
- **Microphone sample rate is auto-detected per device** (`VoiceCommands.InitVosk()` queries `Microphone.GetDeviceCaps` and tells Vosk the real rate) — no manual configuration needed if the laptop's mic doesn't support 16kHz natively (many webcam/laptop mics don't; this was already an issue on the desktop's webcam mic).
- The **Vosk model folder** (`Assets/StreamingAssets/VoskModel/vosk-model-small-en-us-0.15/`, ~68MB) must be committed to git (it was untracked before this session — make sure it's included in the commit, not `.gitignore`d).
- Scene wiring (the `VoiceCommands`/`VoiceProcessor`/`ListeningIndicatorText` GameObjects and their Inspector fields in `MainScene`) is saved directly in `MainScene.unity` — as long as the scene was saved (Ctrl+S) before committing, no manual re-setup is needed on the laptop.
- If the laptop has multiple microphones (e.g. built-in + USB headset), check the **Voice Processor** component's **Microphone Index** field on the `VoiceCommands` GameObject after first Play — Console logs will show which mic is being used if you re-add a debug log, or just verify by testing.

## Files touched this session

- `Assets/Scripts/Voice/VoiceCommands.cs` — main voice command logic (new)
- `Assets/Scripts/Voice/VoiceListeningIndicator.cs` — on-screen listening indicator (new)
- `Assets/Scripts/Voice/ThirdParty/**` — Vosk + VoiceProcessor third-party plugin code (new, vendored)
- `Assets/StreamingAssets/VoskModel/**` — bundled offline speech model (new)
- `Assets/Scripts/Management/Layers.cs` — default startup state now shows skeleton instead of skin
- `Assets/Scripts/Utils/StaticMethods.cs` — fixed `SetActiveRecursively` dropping deeply-nested objects from its "changed" list (was only tracking direct children)
- `Assets/Scripts/Base/CameraController.cs` — added `StartAutoRotate()` for the "rotate"/"slow rotate" commands
