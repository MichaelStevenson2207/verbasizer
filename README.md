# verbasizer

`verbasizer` is a .NET 8 Blazor app inspired by the Bowie / Ty Roberts cut-up workflow. Users can paste multiple text sources into the app, click **Verbasize**, and get lyric-like output that is randomized but still shaped by the punctuation and sentence flow of the original material.

## Features

- Multiple source panels so the generated draft can pull from several texts at once.
- Punctuation-aware lyric generation that keeps commas, pauses, and sentence endings readable.
- Repeatable "click again for a different draft" behavior from the same source bank.
- JSON health check endpoint at `/health`.
- Installable PWA support with a web app manifest and service worker.

## Running locally

```powershell
dotnet run --project .\verbasizer\verbasizer.csproj
```

By default the app runs on the URLs defined in `verbasizer\Properties\launchSettings.json`:

- `http://localhost:5069`
- `https://localhost:7296`

## Using the app

1. Paste text into one or more source boxes.
2. Choose how many lines you want in the generated draft.
3. Click **Verbasize**.
4. Click **Verbasize** again any time you want a new variation from the same source material.

The generator builds a shared token chain from all supplied text, then produces lines from that combined corpus. That gives the app the Bowie-style cut-up feel while keeping punctuation attached to the lyric in a readable way.

## Install as an app

The site now includes Progressive Web App support. In a supported browser, open the app and use the browser's **Install app** or **Add to Home Screen** action to install `verbasizer` like a desktop or mobile app.

## Health check

The app exposes a health endpoint at:

```text
/health
```

Example response:

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

This is useful for local smoke checks, deployment probes, or documenting the app in a repo wiki.

## Additional documentation

- App-specific README: `verbasizer\README.md`
- Wiki-ready feature page: `docs\wiki\Verbasizer.md`
