# Voce-Anatomy: Mac setup for students

How to get Voce-Anatomy running, with voice commands, on an Apple Silicon Mac (M1 to M4).
You run it from inside the Unity Editor; there is no separate app to install.

Plan for about an hour the first time, most of it waiting on downloads and Unity's first
import. You need roughly 15 GB of free disk space and an internet connection during setup.
After setup everything runs offline.

Everything in the app can be done with the mouse. If the voice part (Part 3) gives you
trouble, skip it: the app runs normally without it and you can come back to it later.

## What you are installing

| Piece | Why |
|---|---|
| Unity Hub + Unity Editor **2022.3.22f1** | Runs the project. The exact version matters. |
| Python 3.12 + a few packages | Runs the local speech recognizer (Whisper) that Unity talks to. |
| Whisper speech model (about 1.6 GB) | Downloaded automatically the first time the recognizer starts. |

## Part 1: Install Unity 2022.3.22f1

Use exactly **2022.3.22f1**. It is the version the project was built and tested with. Newer
versions (including Unity 6) have not been tested and will try to upgrade the project when
they open it, which can break it.

1. Download and install **Unity Hub** from https://unity.com/download.
2. Open Unity Hub and sign in with a Unity account (creating one is free). If it asks about
   a license, choose the free **Personal** license (or Student, if you have one).
3. Install the editor. The quickest way is to paste this link into Safari's address bar and
   allow it to open Unity Hub:

   ```
   unityhub://2022.3.22f1/887be4894c44
   ```

   If the link does nothing: in Unity Hub go to **Installs → Install Editor → Archive**,
   follow the link to the download archive, find **2022.3.22f1** under Unity 2022.3 LTS,
   and click its **Unity Hub** button.
4. When asked which build, choose **Apple silicon** (not Intel).
5. On the "Add modules" screen you do not need any extra modules. Untick everything that is
   optional and click **Install**. This is a download of several GB.

When it finishes, Unity Hub's **Installs** tab should list `2022.3.22f1` with an
"Apple silicon" label.

## Part 2: Get the project and open it

1. Unzip the project zip you were given. Move the resulting folder to your home folder and
   make sure it is named `VoceAnatomy`, so it sits at `~/VoceAnatomy`. (Avoid Desktop and
   Documents if you have iCloud syncing on for them.) The rest of this guide assumes that
   location.
2. Open **Terminal** (press Cmd+Space, type `Terminal`, press Return) and run this once.
   macOS marks files from a downloaded zip as untrusted, and that can stop Unity from
   loading the project's bundled plugins:

   ```bash
   xattr -dr com.apple.quarantine ~/VoceAnatomy
   ```

   It prints nothing when it works.
3. In Unity Hub: **Projects → Add → Add project from disk**, pick the `VoceAnatomy` folder.
4. Check that the project row shows editor version **2022.3.22f1**, then click it to open.
   If Unity Hub offers to open it with a different version, cancel and go back to Part 1.
5. Wait. The first open imports every asset and can take 15 minutes or more. Later opens
   are fast.
6. If a window asks to **import TMP Essentials / TextMeshPro Essential Resources**, accept
   it. Without it the on-screen text renders incorrectly.
7. In the **Project** panel at the bottom, open `Assets/Scenes/MainScene` (double-click) if
   it is not already the open scene.
8. Press the **Play** button (the triangle at the top centre). The anatomy model should
   appear in the Game view and respond to the mouse.

macOS may ask whether Unity can use the **camera** and the **microphone**. Allow the
microphone if you want voice. The camera is only used for the optional hand-gesture
control (see Part 5); you can deny it.

If this works, the mouse-only version is done. Press Play again to stop, then continue to
Part 3 for voice.

## Part 3: Set up voice recognition (one time)

Voice uses a small Python program (the "sidecar") that runs on your Mac alongside Unity.
Unity starts and stops it for you, but it has to be installed once.

1. Install **Python 3.12** from https://www.python.org/downloads/macos/ (download the
   "macOS 64-bit universal2 installer" for the latest 3.12.x and run it).
2. In Terminal, check it is there:

   ```bash
   python3.12 --version
   ```

   You should see `Python 3.12.something`.
3. Go to the sidecar folder and create its Python environment:

   ```bash
   cd ~/VoceAnatomy/python/asr_sidecar
   python3.12 -m venv venv
   source venv/bin/activate
   pip install -r requirements.txt
   ```

   After `source venv/bin/activate` your prompt starts with `(venv)`. The `pip install`
   step downloads a few hundred MB and takes a couple of minutes.

   The folder must be called exactly `venv` and must be created on your own Mac. A `venv`
   folder copied from another computer will not work.
4. Start the sidecar by hand this first time, so it can download the speech model:

   ```bash
   python main.py
   ```

   It downloads about 1.6 GB, then loads the model. Leave it alone until you see:

   ```
   Warm-up complete -- ready.
   Serving on http://127.0.0.1:8765
   ```

5. Press **Ctrl+C** in Terminal to stop it. You can close Terminal. You do not need to
   start it by hand again: from now on Unity launches it automatically.

## Part 4: Use it

1. Open the project in Unity and press **Play**.
2. Wait about 10 to 30 seconds after pressing Play. Unity is starting the sidecar and
   loading the model in the background. The **Console** panel shows this line when voice
   is ready:

   ```
   [WhisperAsrBackend] Sidecar reachable and warmed up at http://127.0.0.1:8765.
   ```

3. Click once inside the **Game** view so it has keyboard focus.
4. **Hold the V key, speak, then release.** The command runs a moment after you let go.
   A listening indicator shows on screen while V is held.

Things to try:

- "show muscles", "hide muscles", "show skeleton", "show nerves"
- "show heart", then "focus"
- "zoom in 20 percent", "zoom out"
- "rotate", "slow rotate"
- "show the heart and focus on it" (chain up to three commands with "and" / "then")
- "reset" puts everything back to the start

The full list is in `docs/VOICE_AND_GESTURE_COMMANDS.md` in the project folder.

Speak at a normal pace, fairly close to the laptop. On a Mac the recognizer runs on the
CPU, so expect a short pause (around a second or two) between releasing V and the command
taking effect.

## Part 5: Hand gestures (optional)

The app can also track your hand through the webcam: point your index finger to move the
pointer, pinch thumb and index finger to click. It is on by default if you allowed camera
access. To turn it off, say **"gesture off"**, or simply deny camera access. Mouse and
voice work the same either way.

## Troubleshooting

**Unity Hub wants to open the project with a different Unity version.**
Cancel. Install 2022.3.22f1 (Part 1) and open with that.

**A message says a file such as `libmediapipe_c.dylib` "cannot be opened because the
developer cannot be verified", or the Console shows `DllNotFoundException`.**
Quit Unity, run the `xattr` command from Part 2 step 2 again, and reopen the project.

**The UI text looks like pink or blank boxes.**
In Unity's menu: Window → TextMeshPro → Import TMP Essential Resources.

**Voice does nothing when holding V.** Check these in order:

1. Did you click inside the Game view first?
2. Look in the Console for lines starting with `[WhisperAsrBackend]` or `[VoiceCommands]`.
   They say what went wrong.
3. `Can't auto-start the sidecar: no venv/main.py under ...` means Part 3 step 3 was not
   done, or the project is not where you ran those commands. Redo Part 3 step 3 inside
   this project's `python/asr_sidecar` folder.
4. `Sidecar didn't become ready within 60s` usually means the model has not been downloaded
   yet. Do Part 3 step 4 (run `python main.py` by hand once and wait for "ready").
   The same message gives the path of a log file, `asr_sidecar.log`, with the details.
5. `No microphone detected`, or commands are never recognised: open System Settings →
   Privacy & Security → Microphone and make sure **Unity** (and **Unity Hub**) are switched
   on. Then quit and reopen Unity.
6. A message such as "No microphone device name contains "DJI"" is harmless. It means the
   app is using your Mac's default microphone.

**To check the sidecar on its own**, without Unity:

```bash
cd ~/VoceAnatomy/python/asr_sidecar
source venv/bin/activate
python main.py
```

Wait for "ready", then in a second Terminal window run:

```bash
curl http://127.0.0.1:8765/health
```

A reply of `{"model": "...", "status": "ok"}` means the sidecar is fine and the problem is
on the Unity side. Stop it with Ctrl+C when done.

**Voice works but is too slow, or commands time out.**
Open `python/asr_sidecar/config.yaml` in a text editor, find the `macos:` section, and add
a `model` line so it reads:

```yaml
macos:
  device: cpu
  compute_type: int8
  model: small.en
```

Save, stop Play, press Play again. The smaller model downloads on first use (do Part 3
step 4 again so the download is not cut off) and is faster but a little less accurate.

**`pip install` fails.**
Check that `python3.12 --version` works and that your prompt showed `(venv)` before you ran
`pip`. If not, repeat Part 3 step 3 from the `cd` line.

**Still stuck?** Send your instructor the red error lines from Unity's Console and, for
voice problems, the `asr_sidecar.log` file.
