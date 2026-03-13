# TetraControl2Connect

## Description
Connects multiple Connect sites (even across different organizations) with a single TetraControl instance. All configuration is managed through the built-in Admin UI.

## Beschreibung
Verbindet mehrere Connect-Standorte (auch aus unterschiedlichen Organisationen) mit einer TetraControl-Instanz. Die Konfiguration erfolgt bequem über die integrierte Admin-Oberfläche.

---

## Prerequisites / Voraussetzungen
* .NET 10.0 Runtime or later / .NET 10.0 Runtime oder höher
* TetraControl must be running continuously (preferably on localhost — the WebSocket connection is unencrypted!)
* The web server must be enabled in TetraControl
* A user with sufficient web server permissions must be configured in TetraControl
* Each site requires a "Public Connect API" key — available at https://connect.feuersoftware.com/v2/app/interfaces

## Important Notes / Hinweise
* When vehicles or users are added or modified in Connect, the application must be restarted.
* Data can only be processed when the ISSIs of vehicles and users are correctly stored in Connect.

## Installation & Setup / Installation & Ersteinrichtung

1. Extract the application to a directory / Anwendung in ein Verzeichnis entpacken
2. Start the application — the Admin UI opens at `http://localhost:5050` / Programm starten — die Admin-Oberfläche öffnet sich unter `http://localhost:5050`
3. On first start, the database is populated with defaults / Beim ersten Start wird die Datenbank mit Standardwerten befüllt
4. The setup wizard at `/setup` guides you through the settings / Der Einrichtungsassistent unter `/setup` führt durch die Einstellungen

> **Upgrading:** If an existing `appsettings.json` with configuration sections is present, settings are automatically imported on first start.
>
> **Upgrade:** Wenn eine bestehende `appsettings.json` mit Konfigurationsabschnitten vorhanden ist, werden die Einstellungen beim ersten Start automatisch importiert.

## Admin UI / Admin-Oberfläche

Available at / Erreichbar unter `http://localhost:5050`:

* **Dashboard** — Overview with quick access to all settings / Übersicht mit Schnellzugriff
* **Setup Wizard** (`/setup`) — Step-by-step initial configuration / Schritt-für-Schritt-Ersteinrichtung
* **Settings Pages / Einstellungsseiten:**
  * Program Options / Programmoptionen
  * TetraControl Connection / TetraControl-Verbindung
  * Connect Sites / Connect-Standorte
  * Status Mappings / Status-Zuordnungen
  * Patterns / Muster — Regex for SDS alarm parsing / Regex für SDS-Alarmauswertung
  * Severity Levels / Schweregrade
  * Siren Callout / Sirenen-Alarmierung
  * Siren Status / Sirenen-Status
* **Live View / Live-Ansicht** (`/live`) — Real-time messages / Echtzeit-Nachrichten
* **Backups** (`/backups`) — Configuration backup & restore / Sicherung & Wiederherstellung

## Database / Datenbank

Settings are stored in SQLite (`settings.db`) managed via EF Core Migrations. Daily backups are created automatically.

Einstellungen werden in SQLite (`settings.db`) gespeichert und über EF Core Migrations verwaltet. Tägliche Backups werden automatisch erstellt.

## Web API

REST API at / REST API unter `http://localhost:5050/api/settings/`:

| Method | Path | Description / Beschreibung |
|--------|------|----------------------------|
| `GET` | `/api/settings` | List all sections / Alle Abschnitte |
| `GET/PUT` | `/api/settings/program` | Program options / Programmoptionen |
| `GET/PUT` | `/api/settings/tetracontrol` | TetraControl connection / Verbindung |
| `GET/PUT` | `/api/settings/connect` | Sites, subnets, sirens / Standorte, Subnetze, Sirenen |
| `GET/PUT` | `/api/settings/status` | Status mappings / Zuordnungen |
| `GET/PUT` | `/api/settings/pattern` | Alarm parsing patterns / Alarmauswertung |
| `GET/PUT` | `/api/settings/severity` | Severity translations / Schweregrade |
| `GET/PUT` | `/api/settings/siren-callout` | Siren callout codes / Sirenen-Alarmierung |
| `GET/PUT` | `/api/settings/siren-status` | Siren status codes / Sirenen-Status |
| `POST` | `/api/settings/import` | Import from appsettings.json |

Swagger UI: `http://localhost:5050/swagger`

## How It Works / Funktionsweise

The application connects to TetraControl via WebSocket and processes position data, status data, and SDS messages. Voice calls are ignored.

Das Programm verbindet sich über WebSocket mit TetraControl und verarbeitet Positionsdaten, Statusdaten und SDS. Funkgespräche werden ignoriert.

## Development / Entwicklung

### Prerequisites / Voraussetzungen
* .NET 10.0 SDK
* Node.js 22+ with/mit pnpm
* Git

### Build
```bash
dotnet build
```

### Tests
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

During development, SPA Proxy forwards requests to the Nuxt dev server. For production, `dotnet publish` builds and bundles the SPA.

Im Entwicklungsmodus leitet der SPA-Proxy Anfragen an den Nuxt Dev-Server weiter. Für die Produktion baut `dotnet publish` die SPA und bündelt sie.

### Simulator
```bash
dotnet run --project TetraControl2Connect.Simulator -- [port]
```
WebSocket server simulating the TetraControl protocol. Default port: 8085.

## Architecture / Architektur

### Projects / Projekte
* `TetraControl2Connect` — ASP.NET Core web app (hosted services, Web API, SignalR, Nuxt 3 SPA)
* `TetraControl2Connect.Shared` — Shared option classes and models / Gemeinsame Options-Klassen
* `TetraControl2Connect.Test` — xUnit tests (95+)
* `TetraControl2Connect.Simulator` — TetraControl WebSocket simulator

### Data Storage / Datenhaltung
* **SQLite** (`settings.db`) — Relational tables via EF Core Migrations
* **Auto-seeding** — Imports from `appsettings.json` or seeds defaults on first start
* **Daily backups** — Automatic snapshots / Automatische Sicherungen

### SPA Integration
* **Dev:** `Microsoft.AspNetCore.SpaProxy` launches Nuxt dev server and proxies requests
* **Production:** `dotnet publish` builds the SPA into `wwwroot/`

## License / Lizenz

[GNU Affero General Public License v3.0](LICENSE)

## Copyright
Copyright Feuer Software GmbH

Website: https://feuersoftware.com