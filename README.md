# Voce-Anatomy

Voce-Anatomy is a real-time, hands-free educational tool for exploring a high-resolution, open-source 3D model of human anatomy. Instead of a mouse and keyboard, you speak commands — "show the heart," "rotate," "zoom in 20 percent" — and point and pinch in the air to select and manipulate structures, using nothing but a microphone and a webcam or Leap Motion controller. It's designed for classroom and demo settings where a presenter can navigate the body naturally, without breaking eye contact with an audience to click through menus.

Voce-Anatomy, developed by Steve DiPaola at the research lab: iVizLab, Simon Fraser University.

Voce-Anatomy is based on Unity build of Z-Anatomy by Lluís Vinent Juanico and the Blender Z-Anatomy team.

- Z-Anatomy Unity application developed by Lluís Vinent Juanico ([github.com/LluisV/Z-Anatomy](https://github.com/LluisV/Z-Anatomy)).
- BodyParts3D by Kousaku Okubo, Z-Anatomy design, modelling and anatomy by Gauthier Kervyn, and Blender development by Marcin Zielinski.

Voce-Anatomy adds offline voice commands (local Whisper speech recognition) and hands-free pointer control — via a Leap Motion controller or a plain webcam (MediaPipe) — on top of that Unity build.

Project page: https://ivizlab.github.io/VoceAnatomy

## License

Distributed under **CC-BY-SA 4.0**, consistent with the original Z-Anatomy project. See [LICENSE](LICENSE) for full attributions (models, translations, authors) — some bundled sub-assets carry CC-BY-NC/NC-SA restrictions (non-commercial use only), see LICENSE for details.

## Requirements

- **Unity 2022.3.22f1 LTS** (install via Unity Hub — this exact version is confirmed working; newer versions, including Unity 6, haven't been tested and will try to upgrade the project)
- **Python 3.10+** for the local speech recognizer (see step 4 below)
- A **microphone** for voice commands
- Optional, for hands-free pointer control (pick either):
  - a **Leap Motion controller** with Ultraleap's current Hand Tracking software installed (from [ultraleap.com](https://www.ultraleap.com)) — supports pointing, pinch-select, and rotate/pan/zoom gestures
  - a **webcam** (MediaPipe hand tracking) — pointing and pinch-select only
- An **internet connection** for first-time setup (Unity packages, Python packages, and the ~1.6 GB Whisper model). After setup everything runs offline — no cloud calls, no API keys.
- Verified on **Windows** (NVIDIA GPU recommended for Whisper; CPU works but is slower). macOS on Apple Silicon also works — see [docs/STUDENT_SETUP_MAC.md](docs/STUDENT_SETUP_MAC.md).

MediaPipe is included in this repository and the Ultraleap Unity plugin is pulled in automatically by the Package Manager — no manual plugin installation is required.

## Installing

1. Install [Unity Hub](https://unity.com/download), then install **Unity 2022.3.22f1** through it: Hub → Installs → Install Editor → Archive → find 2022.3.22f1.
2. Clone this repo:
   ```
   git clone https://github.com/ivizlab/VoceAnatomy.git
   ```
3. In Unity Hub: **Add** → select the cloned `VoceAnatomy` folder → open it with 2022.3.22f1. First open will take a while (importing all assets). If prompted to **import TextMeshPro Essential Resources**, accept — the UI text will otherwise render incorrectly.
4. Set up the speech recognizer once per machine (Windows shown; macOS steps are in [python/asr_sidecar/README.md](python/asr_sidecar/README.md)):
   ```
   cd python\asr_sidecar
   python -m venv venv
   venv\Scripts\activate
   pip install -r requirements.txt
   python main.py
   ```
   The first run downloads the Whisper model. Once it prints `Warm-up complete -- ready.`, stop it with Ctrl+C — from then on Unity starts and stops it automatically.
5. Open `Assets/Scenes/MainScene.unity` if it doesn't load automatically, and press Play. Voice is ready a few seconds after Play starts.

Everything in the app also works with mouse and keyboard, so the voice and hand-tracking pieces can be set up later.

## Using voice and gesture input

- **Voice:** hold **V** (or the **mouse middle button**) to talk, release to execute the recognized command — e.g. "show heart", "rotate", "zoom in 20 percent", "show heart and focus". Up to three commands can be chained with "and"/"then".
- **Hand tracking:** switch modes by voice — **"gesture leap"** (Leap Motion), **"gesture video"** (webcam), or **"gesture off"** (mouse only). In either tracking mode, move your hand to steer the cursor and pinch thumb + index finger to click/select. In Leap mode, a closed fist rotates the camera and two hands pan/zoom.

See [docs/VOICE_AND_GESTURE_COMMANDS.md](docs/VOICE_AND_GESTURE_COMMANDS.md) for the full command and gesture list.

Voice settings (which microphone to use, sidecar address/timeouts) live in `Assets/StreamingAssets/asr_config.json`; recognizer settings (model, GPU/CPU) live in `python/asr_sidecar/config.yaml`.

## Repository layout

| Path | What's there |
|---|---|
| `Assets/Scripts/Voice/` | Push-to-talk capture, Whisper backend, transcript normalizer, and the command matcher (`VoiceCommandMatcher.cs` is the single source of truth for the voice vocabulary) |
| `Assets/Scripts/Base/` | Camera, selection, and pointer input, including the Leap (`LeapInputProvider`, `LeapCameraGestures`) and webcam (`GestureInputProvider`) hand-tracking providers |
| `python/asr_sidecar/` | Local faster-whisper HTTP server that Unity sends audio to |
| `docs/` | Command reference, Mac setup guide, design notes, and development logs (Leap integration, ASR migration) |
