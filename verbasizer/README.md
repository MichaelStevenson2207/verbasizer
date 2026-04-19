# verbasizer app

This Blazor app turns multiple source texts into Bowie-style cut-up lyric drafts.

## What users can do

1. Paste text into multiple source boxes.
2. Add more sources with **Add source**.
3. Pick a lyric length from 2 to 12 lines.
4. Click **Verbasize** to generate a new punctuation-aware lyric draft.

The generator mixes all source material into one shared corpus, then walks that corpus to build fresh lines that still read like sentences instead of raw shuffled words.

## Health endpoint

The app exposes:

```text
/health
```

It returns JSON with the overall app status plus the built-in `self` check description.

## Local run

```powershell
dotnet run --project .\verbasizer.csproj
```
