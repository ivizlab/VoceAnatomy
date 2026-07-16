# Voce-Anatomy

Voce-Anatomy is a real-time, hands-free educational tool for exploring a high-resolution, open-source 3D model of human anatomy. Instead of a mouse and keyboard, you speak commands — "show the heart," "rotate," "zoom in 20 percent" — and point and pinch in the air to select and manipulate structures, using nothing but a microphone and a webcam. It's designed for classroom and demo settings where a presenter can navigate the body naturally, without breaking eye contact with an audience to click through menus.

Voce-Anatomy, developed by Steve DiPaola at the research lab: iVizLab, Simon Fraser University.

Voce-Anatomy is based on Unity build of Z-Anatomy by Lluís Vinent Juanico and the Blender Z-Anatomy team.

- Z-Anatomy Unity application developed by Lluís Vinent Juanico ([github.com/LluisV/Z-Anatomy](https://github.com/LluisV/Z-Anatomy)).
- BodyParts3D by Kousaku Okubo, Z-Anatomy design, modelling and anatomy by Gauthier Kervyn, and Blender development by Marcin Zielinski.

Voce-Anatomy adds offline voice commands and MediaPipe-based hand-gesture control on top of that Unity build.

Project page: https://ivizlab.github.io/VoceAnatomy

## License

Distributed under **CC-BY-SA 4.0**, consistent with the original Z-Anatomy project. See [LICENSE](LICENSE) for full attributions (models, translations, authors) — some bundled sub-assets carry CC-BY-NC/NC-SA restrictions (non-commercial use only), see LICENSE for details.

## Requirements

- **Unity 2022.3.22f1 LTS** (install via Unity Hub — this exact version is confirmed working; newer versions haven't been tested)
- A **webcam** (for hand-gesture control via MediaPipe)
- A **microphone** (for voice commands via Vosk — fully offline, no internet connection or API key required at runtime)
- An **internet connection** the first time you open the project in Unity, so the Package Manager can resolve a couple of small package dependencies
- Verified on **Windows**; MediaPipe hand tracking runs CPU-only on this platform (no GPU acceleration path available here), which is sufficient for real-time use but worth knowing if performance seems limited on lower-end hardware

MediaPipe and the Vosk speech model are already included in this repository (not separate downloads) — no manual plugin installation is required.

## Installing

1. Install [Unity Hub](https://unity.com/download), then install **Unity 2022.3.22f1** through it: Hub → Installs → Install Editor → Archive → find 2022.3.22f1.
2. Clone this repo:
   ```
   git clone https://github.com/ivizlab/VoceAnatomy.git
   ```
3. In Unity Hub: **Add** → select the cloned `VoceAnatomy` folder → open it with 2022.3.22f1.
4. First open will take a while (importing all assets). If prompted to **import TextMeshPro Essential Resources**, accept — the UI text will otherwise render incorrectly.
5. Open `Assets/Scenes/MainScene.unity` if it doesn't load automatically.
6. Press Play.

## Using voice and gesture input

- **Voice:** hold **Right Ctrl** to talk, release to execute the recognized command — e.g. "show heart", "rotate", "zoom in 20 percent", "show heart and focus". See `VOICE_COMMANDS.md` for the full list.
- **Gestures:** point your index finger at the screen; pinch (thumb and index finger together) to click/select, same as a mouse click.
  - Toggle hand-tracking on/off by voice ("gestures on" / "gestures off") or via the `enableGestures` checkbox on the `GestureInputProvider` component in the Inspector.
  - If left/right feels mirrored for your webcam, toggle **Mirror X** on the same component.
