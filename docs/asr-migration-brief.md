# Task Brief: Replace VOSK ASR pathway with faster-whisper

## Scope

**This task is only about improving speech recognition accuracy.** Replace the VOSK
engine with faster-whisper. Do not change the command grammar, the command matcher,
or the interaction model. Do not add an LLM layer — that is a possible future project
pending user research, and is explicitly out of scope here.

## Context

This is a Unity 3D anatomy application for physicians and medical educators, driven by:

- **Hand gestures** via Leap Motion (untouched by this task — do not modify)
- **Voice commands** via push-to-talk, currently backed by VOSK offline ASR

Push-to-talk is already implemented and working: a USB wireless mouse middle button
gates audio capture. This is deliberate — the presenter talks to a live audience
between commands, and audience members may speak during the demo. **Push-to-talk must
be preserved exactly as-is.** Do not replace it with VAD-based auto-detection,
continuous listening, or a wake word.

The problem being solved: VOSK misrecognizes commands frequently. The presenter has
to speak unnaturally slowly to get reliable recognition, which hurts live demos.
Fuzzy string matching was added to compensate; that is a workaround for a weak ASR
engine, not a fix. Microsoft Word's dictation transcribes the same speaker at fast,
casual speed with no errors, confirming the bottleneck is VOSK itself — not the
speaker, not the microphone.

**Requirement: everything stays open source and fully local.** No cloud APIs, no
network calls at inference time. This is a lab requirement — the app is intended to
be given to other institutions, and physician privacy is non-negotiable. Do not
introduce any hosted STT service, even as an optional fallback.

## How the existing command system works

This matters for the migration and must be preserved.

The matcher does **keyword extraction from a longer utterance**, not whole-string
matching. It scans recognized text for known keywords and ignores everything else:

- `show <part>` — where `<part>` comes from a known list of anatomy parts
- `focus` — zoom in on the current part
- `and` — separator for chaining multiple commands, executed in order

So the presenter can say *"show me the heart and focus in on it"* and the system
extracts `show ... heart ... and ... focus`, discarding the filler. This gives a
usable first level of casual phrasing today.

**Consequence for this task:** overall word error rate is not the metric that
matters. What matters is whether the **keywords and part names** survive
transcription intact. Filler words can be wrong with no impact.

## Target hardware

- ASUS laptop, AMD Ryzen 7000-series CPU
- NVIDIA RTX 4080, 12 GB VRAM (shared with the Unity render)
- Windows
- Microphone: DJI Mic Mini, receiver connects over USB-C, presents as a standard USB
  audio input device at 48 kHz

---

## Step 0 — Discovery (do this first, report before changing anything)

Do not write implementation code until this is done and reported back.

Inventory the existing voice pathway and produce a short written summary covering:

1. Where audio capture happens, what sample rate and format, and how push-to-talk
   button state is read and gated.
2. Every file, class, and method that touches VOSK — model loading, partial vs. final
   result handling, and any threading or coroutine wrappers.
3. The **full keyword vocabulary**: every command keyword, the complete anatomy part
   list, any parameters and their formats/units, and exactly how `and` chaining is
   parsed.
4. **How the matcher normalizes text before matching** — casing, punctuation
   stripping, tokenization, number handling. This is critical; see the next section.
5. Where the fuzzy-matching layer lives and exactly what it does. Do not delete it.
6. Where matched commands enter the Unity command dispatcher — the seam where ASR
   output becomes application behavior.

Flag anything that assumes streaming/incremental partial results, since Whisper
returns one result per utterance rather than a growing partial transcript. If that
assumption exists, it is the main structural change in this task — report it and
propose options rather than silently restructuring.

---

## Step 1 — Transcript handling (read before implementing)

### Preserve the raw transcript

Whisper's output is richer than VOSK's: it is punctuated, capitalized, and its
punctuation is acoustically derived — commas and periods mark where the speaker
actually paused and how they chunked the utterance.

**The raw transcript is the durable record and must never be modified in place.**
Store it verbatim exactly as Whisper returns it. Everything the matcher needs is
produced as a *separate, derived value*. Do not overwrite, trim, or clean the raw
string at any point in the pipeline.

### Add a derived normalization step

VOSK and Whisper format the same speech differently, and because the matcher does
keyword extraction, formatting differences can break matching even when recognition
is perfect. Handle this in a **normalization step that sits between the ASR output
and the existing matcher**, producing a derived string for matching while leaving the
raw transcript untouched.

Do **not** solve this by rewriting the matcher's logic. The matcher stays as-is so
its behavior remains predictable.

The normalization step must handle:

- **Capitalization.** VOSK emits lowercase; Whisper capitalizes sentences and
  sometimes proper-noun-like anatomical terms. Match case-insensitively.
- **Punctuation attached to keywords.** A part name at the end of a clause may arrive
  as `heart,` or `heart.` and fail an exact token match. Strip punctuation that is
  attached to a token *for matching purposes only*.
- **Clause boundaries — the highest-risk item.** Whisper may render a spoken "and" as
  a **comma** instead of the word, e.g. *"show me the heart, focus in on it."* This
  would silently break command chaining.

  **Convert clause-boundary punctuation into the chain separator rather than
  discarding it.** A comma or period between two command fragments carries the same
  meaning as the spoken word `and`, and must be translated into it, not stripped.
  Verify this behavior explicitly in testing.
- **Numbers.** VOSK tends to spell out numbers; Whisper tends to write digits
  ("forty percent" → "40%"). If any command takes a numeric parameter, accept both
  forms.

Keep normalization in **one clearly identified place**, isolated from both the ASR
code and the matcher, so it can be adjusted or replaced without touching either.

---

## Step 2 — faster-whisper sidecar

### Architecture

Run ASR as a **separate local Python process** exposing a localhost HTTP endpoint.
Do not embed PyTorch/CTranslate2 in the Unity process or bind to it via C# interop.
Unity posts captured audio, gets back text.

The sidecar must be launchable independently of Unity for testing, and Unity must not
hard-crash if it is not running.

### ASR configuration

```
model:                      large-v3-turbo
device:                     cuda
compute_type:               int8_float16
language:                   "en"          # pin it, do not autodetect
vad_filter:                 True
condition_on_previous_text: False
initial_prompt:             <see below>
```

`condition_on_previous_text=False` — each push-to-talk press is an independent
utterance and must not be biased by the previous command.

`vad_filter=True` — drops empty/silent captures. Without it, an accidental button
press recording near-silence can cause Whisper to hallucinate text, injecting a
phantom command mid-demo.

### initial_prompt vocabulary

Build the `initial_prompt` from the **actual anatomy part list and command keywords
found in Step 0**, not a generic anatomy word list. This biases recognition toward
the exact terms the matcher looks for, and is the cheapest accuracy win available.

Keep it in a config file, not hardcoded, so terms can be added as the anatomy library
grows. Note that `initial_prompt` also influences output formatting style, so if
punctuation behavior proves troublesome it can be nudged here as well.

### Audio capture

- Capture at **16 kHz mono**, or resample before sending — Whisper's native rate.
- Select the input device **by name match, not by index.** USB device indices reorder
  between reboots and unplug cycles, which will silently break a demo.
- Make the device name a config value; log the resolved device at startup.
- Document in the README that the DJI Mic Mini's onboard noise cancellation should be
  **off** — aggressive call-style denoising degrades ASR input quality.

### Model warm-up

Load the model and run one throwaway inference at sidecar startup. The first
inference after a cold load is significantly slower, and that must not land on the
presenter's first command in front of an audience. Do not report "ready" to Unity
until warm-up completes.

### Threading and demo safety

- All ASR calls off Unity's main thread. Frame rate must not hitch during recognition.
- Every call to the sidecar needs a **timeout**. On timeout or connection failure,
  fail gracefully: log it, surface a brief non-blocking UI indication, return to idle
  ready state. Never hang, never block input, never throw unhandled. A missed command
  is recoverable in a live demo; a frozen app is not.
- **Log every utterance as three distinct values: the verbatim raw transcript, the
  normalized string, and the resolved command.** This is the primary tuning and
  debugging tool — when a command misfires, it must be possible to tell whether ASR
  misheard it, normalization mangled it, or the matcher missed it. The raw transcript
  must be logged unaltered, including its original punctuation and casing.

### Rollback safety

Put the ASR backend behind a **config flag or interface** with `vosk` and `whisper`
implementations. Keep VOSK functional and selectable until the new path is validated
in a real rehearsal. Do not delete VOSK code in this task.

Likewise **leave the fuzzy-matching layer in place**, but make it toggleable.
Whisper produces a different distribution of errors than VOSK, so the fuzzy layer may
help, may be neutral, or may actively cause harm by "correcting" an already-correct
transcript. Measure both configurations before deciding.

---

## Acceptance criteria

- Presenter can speak at **normal, fast, casual speed** and have commands recognized
  reliably. Eliminating the need to speak slowly is the entire point of this task.
- **Keyword and part-name recall is the metric**, not overall word error rate.
  Verify against the full anatomy part list, including similar-sounding parts.
- `and` chaining still works, including when Whisper renders the separator as a comma
  or period instead of the word.
- The raw transcript is preserved verbatim end to end and is recoverable for every
  utterance, independent of whatever the matcher did with it.
- Normalization is isolated in one place and can be modified without touching the ASR
  code or the matcher.
- Latency from button release to Unity action feels immediate.
- Unity frame rate is unaffected during recognition.
- Sidecar down or timing out does not break the app.
- VOSK remains selectable via config for rollback.

## Constraints and non-goals

- **Do not modify the Leap Motion / gesture code.**
- **Do not change push-to-talk** to VAD, continuous listening, or wake-word.
- **Do not change the command grammar or the matcher's logic.** Normalization goes in
  front of it, as a derived step.
- **Do not discard information coming out of the ASR.** Anything the matcher does not
  currently use is still preserved in the raw transcript rather than stripped away.
- **Do not add an LLM layer.** Out of scope.
- **Do not refactor unrelated systems.** Keep the diff scoped to the ASR pathway, the
  new normalization step, and their seam into the command dispatcher.
- **No cloud or network-dependent components.**
- Check licenses before adding any dependency — the app is intended for
  redistribution to other institutions.

## Working agreement

- Report after Step 0 with the discovery summary and a proposed file-level change
  plan. Wait for approval before implementing.
- Keep the keyword vocabulary and anatomy part list in **one place** as a single
  source of truth, rather than duplicated across the matcher, the config, and the
  `initial_prompt`. Duplicated lists drift.
- Keep the four stages — **capture, transcribe, normalize+match, execute** — as
  separately testable units. In particular, if deciding *what* command was spoken is
  currently fused with *performing* it, report that in Step 0 and propose separating
  them. Each stage should be testable without running the ones before it: matching
  should be testable by feeding it text, with no microphone and no Unity scene.
- Where you have to guess at intent, ask instead.
