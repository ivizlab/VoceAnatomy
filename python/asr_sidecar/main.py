"""faster-whisper sidecar for Voce-Anatomy's push-to-talk voice commands.

Standalone local process: Unity POSTs one utterance's worth of WAV audio per push-to-talk
press, this returns one JSON transcript. No cloud calls, no network dependency at inference
time -- everything runs on this machine. See ../../asr-migration-brief.md for the full design.

Launch independently of Unity for testing:
    python main.py
    curl http://127.0.0.1:8765/health
    curl -X POST --data-binary @sample.wav -H "Content-Type: audio/wav" http://127.0.0.1:8765/transcribe
"""
import io
import json
import logging
import sys
import wave
from pathlib import Path

import numpy as np
import yaml
from flask import Flask, request, jsonify
from faster_whisper import WhisperModel

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
log = logging.getLogger("asr_sidecar")

CONFIG_PATH = Path(__file__).parent / "config.yaml"


def load_config():
    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        config = yaml.safe_load(f)

    # One config file serves both the Windows/NVIDIA machine and Macs -- see the `macos`
    # section's comment in config.yaml.
    if sys.platform == "darwin":
        config.update(config.get("macos") or {})
    return config


def build_initial_prompt(config):
    """Builds initial_prompt from the vocabulary Unity's VoiceCommandMatcher exports (the
    actual command keywords/anatomy part list, kept as a single source of truth there) rather
    than a hand-maintained duplicate here. Falls back to a small built-in vocabulary if the
    file isn't present yet, so this sidecar stays launchable/testable on its own."""
    vocab_path = (Path(__file__).parent / config["vocabulary_path"]).resolve()
    phrases = None

    if vocab_path.exists():
        try:
            with open(vocab_path, "r", encoding="utf-8") as f:
                vocab = json.load(f)
            phrases = vocab.get("phrases")
        except Exception as e:
            log.warning("Failed to read vocabulary file %s: %s -- using fallback vocabulary.", vocab_path, e)
    else:
        log.warning(
            "Vocabulary file not found at %s -- using fallback vocabulary. It's exported by "
            "Unity's VoiceCommandMatcher at startup; run the Unity app once (pointed at the "
            "same asr_config.json) to generate it, then restart this sidecar.",
            vocab_path,
        )

    if not phrases:
        phrases = config.get("initial_prompt_fallback", [])

    prompt = config.get("initial_prompt_prefix", "").strip()
    if phrases:
        prompt = (prompt + " " + ", ".join(phrases)).strip()

    preview = prompt[:200] + ("..." if len(prompt) > 200 else "")
    log.info("initial_prompt (%d chars, %d phrases): %s", len(prompt), len(phrases), preview)
    return prompt


def wav_bytes_to_float32(wav_bytes):
    """Decodes a 16-bit PCM WAV (as produced by Unity's WhisperAsrBackend) into a float32 numpy
    array in [-1, 1], the format faster-whisper expects for an in-memory array. Uses the
    stdlib `wave` module rather than soundfile, so there's no extra native (libsndfile)
    dependency to install."""
    with wave.open(io.BytesIO(wav_bytes), "rb") as wf:
        channels = wf.getnchannels()
        sample_width = wf.getsampwidth()
        frames = wf.readframes(wf.getnframes())

    if sample_width != 2:
        raise ValueError(f"Expected 16-bit PCM WAV, got sample width {sample_width}")

    audio = np.frombuffer(frames, dtype=np.int16).astype(np.float32) / 32768.0
    if channels > 1:
        audio = audio.reshape(-1, channels).mean(axis=1)
    return audio


class Sidecar:
    def __init__(self, config):
        self.config = config

        log.info("Loading model %s (device=%s, compute_type=%s)...",
                  config["model"], config["device"], config["compute_type"])
        self.model = WhisperModel(config["model"], device=config["device"], compute_type=config["compute_type"])
        self.initial_prompt = build_initial_prompt(config)

        # The first inference after a cold load is significantly slower -- run one throwaway
        # inference now so that cost doesn't land on the presenter's first real command.
        log.info("Warming up with a throwaway inference...")
        self._transcribe(np.zeros(16000, dtype=np.float32))  # 1s of silence
        log.info("Warm-up complete -- ready.")

    def _transcribe(self, audio):
        segments, info = self.model.transcribe(
            audio,
            language=self.config["language"],
            vad_filter=self.config["vad_filter"],
            condition_on_previous_text=self.config["condition_on_previous_text"],
            initial_prompt=self.initial_prompt,
        )
        # Raw, unmodified concatenation -- Unity's normalization step (not this sidecar) is
        # responsible for anything downstream of the raw transcript.
        text = "".join(segment.text for segment in segments).strip()
        return text, info

    def transcribe_wav(self, wav_bytes):
        audio = wav_bytes_to_float32(wav_bytes)
        return self._transcribe(audio)


def create_app(sidecar):
    app = Flask(__name__)

    @app.route("/health", methods=["GET"])
    def health():
        # Nothing answers this route until Sidecar.__init__ (model load + warm-up) has already
        # completed, since app.run() below only happens after that -- so a successful /health
        # reply is itself the "ready" signal Unity's WhisperAsrBackend.Initialize() waits on.
        return jsonify({"status": "ok", "model": sidecar.config["model"]})

    @app.route("/transcribe", methods=["POST"])
    def transcribe():
        try:
            text, info = sidecar.transcribe_wav(request.get_data())
        except Exception as e:
            log.exception("Transcription failed")
            return jsonify({"error": str(e)}), 500

        log.info('Transcribed: "%s"', text)
        return jsonify({"text": text, "language": info.language, "duration": info.duration})

    return app


def main():
    config = load_config()
    sidecar = Sidecar(config)
    app = create_app(sidecar)
    log.info("Serving on http://%s:%s", config["host"], config["port"])
    app.run(host=config["host"], port=config["port"], threaded=True, debug=False)


if __name__ == "__main__":
    main()
