# ASR sidecar (faster-whisper)

Local-only HTTP server that Unity's `WhisperAsrBackend` posts push-to-talk audio to and gets
a transcript back from. Runs entirely offline -- no cloud calls, no network dependency at
inference time. Launchable and testable independently of Unity.

## Setup

```bash
python -m venv venv
venv\Scripts\activate        # Windows
pip install -r requirements.txt
```

On macOS:

```bash
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
python main.py               # once by hand -- see below
```

The venv is per-machine (it can't be copied between computers or OSes), so each Mac needs
this done once. Run `python main.py` by hand the first time: it downloads the model (~1.6GB)
from Hugging Face, which takes longer than the 60s Unity waits for an auto-started sidecar.
Once it logs "Warm-up complete -- ready." stop it with Ctrl+C; from then on Unity starts it
automatically.

On Windows, requires an NVIDIA GPU + CUDA-capable PyTorch/CTranslate2 stack for the
`device: cuda` config (the target hardware -- RTX 4080, 12GB VRAM -- has ample headroom for
`large-v3-turbo` at `int8_float16`). Set `device: cpu` in `config.yaml` for testing without a
GPU (much slower). On macOS the `macos` section of `config.yaml` switches to CPU automatically
-- there is no GPU backend for CTranslate2 on Macs.

## Running

Unity starts this automatically: if nothing answers at the configured `baseUrl` when the app
starts, `WhisperAsrBackend` launches `venv\Scripts\python.exe main.py` (`venv/bin/python
main.py` on macOS) from this folder, waits
for warm-up, and stops it again when the app exits. Its output goes to `asr_sidecar.log` in
Unity's `Application.persistentDataPath`. Controlled by `autoStartSidecar` /
`sidecarDirectory` / `sidecarStartupTimeoutSeconds` in `Assets/StreamingAssets/asr_config.json`.

To run it by hand instead (a sidecar that's already running is reused and left running):

```bash
python main.py
```

Loads the model and runs one throwaway inference before it starts serving -- the first real
inference after a cold load is significantly slower, and this keeps that cost off the
presenter's first command. Nothing responds on the configured port until warm-up is done.

Test it without Unity:

```bash
curl http://127.0.0.1:8765/health
curl -X POST --data-binary @sample.wav -H "Content-Type: audio/wav" http://127.0.0.1:8765/transcribe
```

`sample.wav` needs to be 16-bit PCM (any sample rate/channel count -- `main.py` downmixes to
mono; Unity always sends 16kHz mono, resampling on its side if the mic captured at a
different rate).

## Vocabulary / initial_prompt

`config.yaml`'s `vocabulary_path` points at `Assets/StreamingAssets/asr_vocabulary.json` in
the Unity project -- Unity's `VoiceCommandMatcher` exports it at startup from the actual
command keyword/anatomy part list (the single source of truth lives in
`VoiceCommandMatcher.cs`, not here). If that file isn't there yet (e.g. testing this sidecar
before Unity has ever run), `initial_prompt_fallback` in `config.yaml` is used instead and a
warning is logged -- adjust the relative path if your Unity project isn't a sibling of this
folder.

## Mic quality note

Turn off the DJI Mic Mini's onboard noise cancellation/call-style denoising -- aggressive
denoising tuned for voice calls degrades ASR input quality rather than helping it.

## Config

See `config.yaml` for the pinned model/decoding settings (`language: en`,
`condition_on_previous_text: false` since each push-to-talk press is an independent
utterance, `vad_filter: true` so an accidental button press on near-silence doesn't cause
Whisper to hallucinate a phantom command).

## Licenses

All dependencies are permissively licensed and fine to redistribute alongside this app:
- [faster-whisper](https://github.com/SYSTRAN/faster-whisper) -- MIT
- [CTranslate2](https://github.com/OpenNMT/CTranslate2) (faster-whisper's inference engine) -- MIT
- [Flask](https://flask.palletsprojects.com/) -- BSD-3-Clause
- [NumPy](https://numpy.org/) -- BSD-3-Clause
- [PyYAML](https://pyyaml.org/) -- MIT

The Whisper model weights themselves (`large-v3-turbo`, downloaded automatically on first run
via Hugging Face) are released by OpenAI under MIT.
