# 🌈 WLED Manager

Automated housekeeping for a fleet of [WLED](https://kno.wled.ge/) devices: scheduled backups, cross-device preset synchronization, and a minimal HTTP API you can call from your own automations.

> **Security note**: the API is intentionally lightweight and ships with Swagger UI always enabled. Keep the container on a trusted network segment (exactly like your WLED controllers) or place it behind your own reverse proxy/authentication layer.

## 🧭 Table of contents

- [✨ Highlights](#-highlights)
- [⚙️ How it works](#%EF%B8%8F-how-it-works)
- [🚀 Quick start](#-quick-start)
- [🛠 Configuration reference](#-configuration-reference)
- [🌐 HTTP API](#-http-api)

## ✨ Highlights

- **Nightly backups** – persist `cfg.json` and `presets.json` for every configured controller, optionally pinging an external health-check endpoint once the run succeeds.
- **Preset fan-out** – mirror presets from one “source” controller to any number of target strips, with automatic LED-count fixes so presets always match the destination hardware.
- **On-demand API** – trigger backups or preset syncs immediately over HTTP and inspect the full OpenAPI spec at `/swagger`.
- **Health awareness** – every run can notify services such as healthchecks.io and will mark runs as failed if a device cannot be reached.
- **Config hot reload** – drop a `config.json` next to the container volume and changes are applied without restarts.

## ⚙️ How it works

| Component | Responsibility |
| --- | --- |
| `BackupService` | Background worker that waits until the configured `TimeOfDay`, fetches `cfg.json`+`presets.json` from each device via the WLED JSON API, and writes them under the backup path (default `/wled-manager/backups/<device>/`). |
| `PresetsSyncService` | Loads presets from each configured source, normalizes LED segment lengths, and posts them to the target devices using the `/upload` endpoint. Runs once at startup and whenever the `Sync` configuration changes. |
| API controllers | `/api/backups/run` and `/api/presets/sync` simply forward to the services above so you can trigger runs manually or through automations. |
| `HealthChecksService` | Optional healthcheck pings (`start`, `ping`, `fail`) around every backup batch. Compatible with [healthchecks.io](https://healthchecks.io/) and similar services. |

The ASP.NET host listens on `http://0.0.0.0:8080` inside the container. Swagger UI is always enabled for convenience (`/swagger`).

## 🚀 Quick start

### 1. Create a configuration file

`config.json` lives in the `/wled-manager` volume (or next to `appsettings.Development.json` during local development). Example:

```jsonc
{
  "Backup": {
    "Path": "/wled-manager/backups",
    "TimeOfDay": "01:00:00",
    "StartDelay": "00:00:30",
    "Devices": [
      "kitchen-leds-01.home.tld",
      "living-leds-01.home.tld"
    ],
    "Health": "https://hc-ping.example.com/uuid"
  },
  "Sync": [
    {
      "Source": "living-leds-01.home.tld",
      "Targets": [
        "kitchen-leds-01.home.tld",
        "office-leds-01.home.tld"
      ]
    }
  ]
}
```

### 2. Run with Docker Compose (recommended)

```yaml
services:
  wled-manager:
    image: ghcr.io/dr1rrb/wled-manager:latest
    container_name: wled-manager
    ports:
      - "8080:8080"          # API + Swagger
    environment:
      - TZ=America/Toronto
    volumes:
      - ./data/config.json:/wled-manager/config.json:ro
      - ./data/backups:/wled-manager/backups
    restart: unless-stopped
```

Notes:

- The container expects *read/write* access to `/wled-manager/backups` and read access to `config.json`.
- Map the API port to your LAN (or keep it internal and hit it via Docker network).
- Existing sample backups under `src/WledManager/Backups/*` illustrate the folder structure the service creates.

### 3. Trigger a run

Backups happen automatically at `TimeOfDay`, but you can poke the API right away:

```powershell
curl -X POST http://localhost:8080/api/backups/run
curl -X POST http://localhost:8080/api/presets/sync
```


## 🛠 Configuration reference

### `Backup`

| Setting | Type | Default | Description |
| --- | --- | --- | --- |
| `Path` | string | `/wled-manager/backups` | Root folder for captured `config.json` and `presets.json` files. |
| `TimeOfDay` | `HH:mm:ss` | `03:00:00` | Local time when the scheduled backup executes every day. |
| `StartDelay` | `HH:mm:ss` or `null` | `00:00:30` | Delay after startup before running the very first backup. Set to `null` to disable the immediate run. |
| `Devices` | array of strings | `[]` | Hostnames/IPs of WLED controllers accessible from the container. HTTPS is supported; prefixes are optional (e.g., `living.local` becomes `http://living.local`). |
| `Health` | URI or `null` | `null` | Optional health-check endpoint that receives `start`, periodic `ping`, and `fail` calls. |

Full C# model: [`BackupOptions`](src/WledManager/Backups/BackupOptions.cs).

### `Sync`

Array of [`PresetsSyncOptions`](src/WledManager/Synchronization/PresetsSyncOptions.cs):

| Setting | Type | Description |
| --- | --- | --- |
| `Source` | string (required) | Device whose presets should be treated as the canonical definition. |
| `Targets` | array of strings | Controllers that will receive the presets. LED counts are auto-adjusted per target based on their reported hardware configuration. |

Configuration changes are detected at runtime, so you can edit `config.json` and the hosted services will pick up the new list automatically.

## 🌐 HTTP API

| Method & path | Description |
| --- | --- |
| `POST /api/backups/run` | Forces an immediate backup of every configured device. Useful for testing or when integrating with automations (Home Assistant, cron, etc.). |
| `POST /api/presets/sync` | Copies presets from each configured source to its targets. |

Swagger/OpenAPI is available at `/swagger` with inline documentation generated from the C# XML comments.
