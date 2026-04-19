# Verbasizer

## Overview

`verbasizer` is a Bowie / Ty Roberts-inspired cut-up writing tool built with Blazor. Instead of scrambling text into unreadable fragments, it merges multiple source passages and generates lyric-style output that keeps punctuation and sentence rhythm intact.

## User workflow

1. Paste one or more source texts into the source panels.
2. Add more panels when you want to pull from more material.
3. Choose how many lines to generate.
4. Click **Verbasize** for a new draft.
5. Click it again to reshuffle the same source material into a different lyric variation.

## How generation works

- Source texts are tokenized into words and punctuation.
- The app builds transitions between tokens across the combined source corpus.
- New lines start from sentence-like openings and continue through likely next tokens.
- The generator preserves punctuation placement so the result stays readable.
- Each run produces a different mix while still sounding grounded in the original material.

## Health check

The app exposes a simple JSON health endpoint:

```text
/health
```

Example payload:

```json
{
  "status": "Healthy",
  "timestampUtc": "2026-04-19T09:32:00+00:00",
  "durationMilliseconds": 0.1234,
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": "The lyric engine is ready."
    }
  ]
}
```

This can be used for deployment probes, local smoke tests, or status checks in a hosted environment.
