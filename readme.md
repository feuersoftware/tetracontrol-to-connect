# TetraControl2Connect

[🇩🇪 Deutsche Version](readme.de.md)

## Description
Connects multiple Connect sites (even across different organizations) with a single TetraControl instance. All configuration is managed through the built-in Admin UI.

## Prerequisites
* .NET 10.0 Runtime or later
* TetraControl must be running continuously (preferably on localhost — the WebSocket connection is unencrypted!)
* The web server must be enabled in TetraControl
* A user with sufficient web server permissions must be configured in TetraControl
* Each site requires a "Public Connect API" key — available in the Connect portal at https://connect.feuersoftware.com/v2/app/interfaces

## Important Notes
* When vehicles or users are added or modified in Connect, the application must be restarted.
* Data can only be processed when the ISSIs of vehicles and users are correctly stored in Connect.

## Installation & Initial Setup

1. Extract the application to a directory
2. Start the application — the Admin UI opens automatically at `http://localhost:5050`
3. On first start, the database is populated with default values
4. The setup wizard at `/setup` guides you through the essential settings

> **Upgrading from an older version:** If an existing `appsettings.json` with configuration sections is present, settings are automatically imported into the database on first start. Afterwards, the `appsettings.json` can be reduced to logging configuration only.

## Admin UI

The application provides a web-based Admin UI at `http://localhost:5050`:

* **Dashboard** — Overview of all settings sections with quick access
* **Setup Wizard** (`/setup`) — Step-by-step configuration for initial setup
* **Settings Pages** — Editors for all 8 configuration sections:
  * Program Options — Control which data types are transmitted and configure timeouts
  * TetraControl Connection — Host, port, credentials
  * Connect Sites — API keys, subnet addresses, sirens
  * Status Mappings — Pager status values for availability and operation responses
  * Patterns — Regex patterns for SDS alarm parsing
  * Severity Levels — Translation of alarm severity levels to keywords
  * Siren Callout — Control code translation for siren alarms
  * Siren Status — Error code translation for siren monitoring
* **Live View** (`/live`) — Real-time messages from TetraControl
* **Backups** (`/backups`) — Configuration backup and restore

## Database

All settings are stored in a SQLite database (`settings.db`) managed via EF Core Migrations. The database is automatically created and populated on first start.

Daily configuration backups are created automatically and can be managed through the Admin UI.

## Web API

The REST API is available at `http://localhost:5050/api/settings/`:

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/settings` | List all sections |
| `GET/PUT` | `/api/settings/program` | Program options |
| `GET/PUT` | `/api/settings/tetracontrol` | TetraControl connection |
| `GET/PUT` | `/api/settings/connect` | Sites, subnets, sirens |
| `GET/PUT` | `/api/settings/status` | Status mappings |
| `GET/PUT` | `/api/settings/pattern` | Alarm parsing patterns |
| `GET/PUT` | `/api/settings/severity` | Severity translations |
| `GET/PUT` | `/api/settings/siren-callout` | Siren callout codes |
| `GET/PUT` | `/api/settings/siren-status` | Siren status codes |
| `POST` | `/api/settings/import` | Import from appsettings.json |

Swagger UI: `http://localhost:5050/swagger`

## How It Works
The application connects to the TetraControl web server via WebSocket and processes position data, status data, and SDS messages. Voice calls are ignored.

## Development

### Prerequisites
* .NET 10.0 SDK
* Node.js 22+ with pnpm
* Git

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Admin UI (Nuxt 3)
```bash
cd TetraControl2Connect/AdminUI
pnpm install
pnpm run dev          # Dev server on :3000 (proxies to .NET on :5050)
pnpm run generate     # SPA build → .output/public/
pnpm test             # Playwright E2E tests
```

During development, the SPA proxy in the .NET app automatically forwards requests to the Nuxt dev server. For production, `dotnet publish` builds the Admin UI and serves it as a static SPA from `wwwroot/`.

### Simulator
```bash
dotnet run --project TetraControl2Connect.Simulator -- [port]
```
Starts a WebSocket server that simulates the TetraControl protocol. Default port: 8085.

## Architecture

### Projects
* `TetraControl2Connect` — ASP.NET Core web app with hosted services, Web API, SignalR, and Admin UI (Nuxt 3 SPA)
* `TetraControl2Connect.Shared` — Shared option classes and models
* `TetraControl2Connect.Test` — xUnit tests (95+)
* `TetraControl2Connect.Simulator` — TetraControl WebSocket simulator

### Data Storage
* **SQLite** (`settings.db`) — Relational tables for all settings, managed via EF Core Migrations
* **Automatic seeding** — On first start: imports from existing `appsettings.json` or seeds with defaults
* **Daily backups** — Automatic snapshots on configuration changes

### SPA Integration
* **Development:** `Microsoft.AspNetCore.SpaProxy` automatically launches the Nuxt dev server and proxies requests
* **Production:** `dotnet publish` builds the SPA and copies it to `wwwroot/`; ASP.NET Core serves it as static files

## License

This project is licensed under the [GNU Affero General Public License v3.0](LICENSE).

## Copyright
Copyright Feuer Software GmbH

Website: https://feuersoftware.com

All rights reserved.