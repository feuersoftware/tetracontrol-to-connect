# TetraControl2Connect

## Deutsch

### Produktüberblick
TetraControl2Connect verbindet mehrere Connect-Standorte – auch organisationsübergreifend – mit einer einzelnen TetraControl-Instanz. Die Anwendung verarbeitet Positionsdaten, Statusdaten und SDS-Nachrichten aus TetraControl über WebSocket und stellt sie in Connect bereit. Die gesamte Konfiguration erfolgt über die integrierte Admin-Oberfläche; Funkgespräche werden nicht verarbeitet.

### Voraussetzungen für den Betrieb
- .NET 10.0 Runtime oder neuer
- TetraControl läuft dauerhaft und der Webserver ist aktiviert
- Ein TetraControl-Benutzer mit ausreichenden Webserver-Berechtigungen
- Pro Connect-Standort ein „Public Connect API“-Schlüssel: https://connect.feuersoftware.com/v2/app/interfaces
- Fahrzeuge und Benutzer müssen in Connect mit den korrekten ISSIs gepflegt sein

**Wichtige Hinweise**
- Wenn Fahrzeuge oder Benutzer in Connect neu angelegt oder geändert werden, ist ein Neustart der Anwendung erforderlich.
- Die WebSocket-Verbindung zu TetraControl ist unverschlüsselt. TetraControl sollte deshalb möglichst lokal oder in einem vertrauenswürdigen Netz betrieben werden.

### Installation und erste Einrichtung
1. Anwendung in ein Verzeichnis entpacken oder das Repository lokal auschecken.
2. Anwendung starten, zum Beispiel aus dem Repository mit:
   ```bash
   dotnet run --project TetraControl2Connect
   ```
3. Nach dem Start versucht die Anwendung, die Admin-Oberfläche unter `http://localhost:5050` im Browser zu öffnen.
4. Beim ersten Start wird die SQLite-Datenbank `settings.db` angelegt. Wenn bereits passende Konfigurationsabschnitte in `appsettings.json` vorhanden sind, werden diese automatisch importiert; andernfalls werden Standardwerte angelegt.
5. Der Einrichtungsassistent unter `http://localhost:5050/setup` führt durch die Grundkonfiguration.

### Admin-Oberfläche und API
Die Admin-Oberfläche unter `http://localhost:5050` bietet unter anderem:
- Dashboard mit Schnellzugriff auf alle Konfigurationsbereiche
- Einrichtungsassistent unter `/setup`
- Einstellungsseiten für Programmoptionen, TetraControl-Verbindung, Connect-Standorte, Status-Zuordnungen, Muster, Schweregrade, Sirenen-Alarmierung und Sirenen-Status
- Live-Ansicht unter `/live` für Echtzeit-Nachrichten
- Backup-Verwaltung unter `/backups` für Sicherung und Wiederherstellung
- Swagger UI unter `http://localhost:5050/swagger`

Die Einstellungen werden in SQLite (`settings.db`) gespeichert und per EF Core Migrationen verwaltet. Zusätzlich zur Backup-Verwaltung in der Oberfläche wird automatisch eine tägliche Sicherung angelegt.

Die REST API ist unter `http://localhost:5050/api` verfügbar:

| Methode | Pfad | Zweck |
|--------|------|-------|
| `GET` | `/api/settings` | Übersicht aller Konfigurationsbereiche |
| `GET/PUT` | `/api/settings/program` | Programmoptionen |
| `GET/PUT` | `/api/settings/tetracontrol` | TetraControl-Verbindung |
| `GET/PUT` | `/api/settings/connect` | Standorte, Subnetze und Sirenen |
| `GET/PUT` | `/api/settings/status` | Status-Zuordnungen |
| `GET/PUT` | `/api/settings/pattern` | Muster für die SDS-Alarmauswertung |
| `GET/PUT` | `/api/settings/severity` | Schweregrade und Übersetzungen |
| `GET/PUT` | `/api/settings/siren-callout` | Sirenen-Alarmierung |
| `GET/PUT` | `/api/settings/siren-status` | Sirenen-Status |
| `POST` | `/api/settings/import` | Einstellungen aus `appsettings.json` importieren |
| `GET` | `/api/backups` | Sicherungen auflisten |
| `POST` | `/api/backups` | Manuelle Sicherung erstellen |
| `POST` | `/api/backups/{id}/restore` | Sicherung wiederherstellen |
| `DELETE` | `/api/backups/{id}` | Sicherung löschen |

### Entwicklung
#### Voraussetzungen
- .NET 10.0 SDK
- Node.js 22+ mit pnpm
- Git

#### Kompilieren, testen und veröffentlichen
```bash
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet publish
```

`dotnet publish` baut zusätzlich die Admin-Oberfläche und kopiert die erzeugten Dateien nach `wwwroot/`.

#### Admin-Oberfläche (Nuxt 3)
```bash
cd TetraControl2Connect/AdminUI
pnpm install
pnpm run dev
pnpm run generate
pnpm test
```

Im Entwicklungsmodus läuft die Admin-Oberfläche unter `http://localhost:3000`. Die Pfade `/api` und `/hubs` werden dabei an `http://localhost:5050` weitergeleitet. `pnpm run generate` erzeugt die statische Ausgabe in `.output/public/`.

### Simulator
```bash
dotnet run --project TetraControl2Connect.Simulator -- [port]
```

- Standardport: `8085`
- WebSocket-Endpunkt: `ws://localhost:{port}/live.json`
- Standardanmeldung: `Connect / Connect`
- Vor dem Start muss bestätigt werden, dass keine produktive Connect-Umgebung betroffen ist.
- Der Simulator stellt interaktive Testszenarien für Brand, Verkehrsunfall, Sirenentest, Verfügbarkeitsänderungen und kontinuierliche Zufallsnachrichten bereit.

### Architektur und Repository-Struktur
- `TetraControl2Connect` – ASP.NET Core Webanwendung mit Einstellungen-API, SignalR-Hub unter `/hubs/messages` und integrierter Nuxt-Admin-Oberfläche
- `TetraControl2Connect/AdminUI` – Nuxt-3-Frontend auf Basis von `@nuxt/ui`
- `TetraControl2Connect.Shared` – Gemeinsame Optionsklassen und Modelle
- `TetraControl2Connect.Test` – xUnit-Testprojekt
- `TetraControl2Connect.Simulator` – WebSocket-Simulator für TetraControl
- Konfigurationsdaten werden in `settings.db` gespeichert; Logdateien liegen im Verzeichnis `logs/`.

### Lizenz
[GNU Affero General Public License v3.0](LICENSE)

Copyright Feuer Software GmbH  
Website: https://feuersoftware.com

---

## English

### Product overview
TetraControl2Connect connects multiple Connect sites – including sites across different organizations – to a single TetraControl instance. The application consumes position data, status data, and SDS messages from TetraControl over WebSocket and forwards them to Connect. All configuration is handled through the built-in Admin UI; voice calls are ignored.

### Runtime prerequisites
- .NET 10.0 Runtime or later
- TetraControl must be running continuously and its web server must be enabled
- A TetraControl user with sufficient web server permissions
- One “Public Connect API” key per Connect site: https://connect.feuersoftware.com/v2/app/interfaces
- Vehicles and users must have the correct ISSIs stored in Connect

**Important notes**
- Restart the application after vehicles or users are added or changed in Connect.
- The WebSocket connection to TetraControl is unencrypted, so TetraControl should ideally run locally or inside a trusted network.

### Installation and initial setup
1. Extract the application to a directory or clone the repository locally.
2. Start the application, for example from the repository root with:
   ```bash
   dotnet run --project TetraControl2Connect
   ```
3. After startup, the application tries to open the Admin UI at `http://localhost:5050` in your browser.
4. On first start, the SQLite database `settings.db` is created. If matching configuration sections already exist in `appsettings.json`, they are imported automatically; otherwise the application seeds sensible defaults.
5. The setup wizard at `http://localhost:5050/setup` guides you through the initial configuration.

### Admin UI and API
The Admin UI at `http://localhost:5050` includes:
- A dashboard with quick access to all configuration areas
- A setup wizard at `/setup`
- Settings pages for program options, the TetraControl connection, Connect sites, status mappings, patterns, severity levels, siren callout settings, and siren status settings
- A live view at `/live` for real-time messages
- Backup management at `/backups` for creating and restoring configuration snapshots
- Swagger UI at `http://localhost:5050/swagger`

Settings are stored in SQLite (`settings.db`) and managed through EF Core migrations. In addition to the backup tools in the UI, the application also creates a daily backup automatically.

The REST API is available at `http://localhost:5050/api`:

| Method | Path | Purpose |
|--------|------|---------|
| `GET` | `/api/settings` | Overview of all configuration sections |
| `GET/PUT` | `/api/settings/program` | Program options |
| `GET/PUT` | `/api/settings/tetracontrol` | TetraControl connection |
| `GET/PUT` | `/api/settings/connect` | Sites, subnet addresses, and sirens |
| `GET/PUT` | `/api/settings/status` | Status mappings |
| `GET/PUT` | `/api/settings/pattern` | SDS alarm parsing patterns |
| `GET/PUT` | `/api/settings/severity` | Severity levels and translations |
| `GET/PUT` | `/api/settings/siren-callout` | Siren callout configuration |
| `GET/PUT` | `/api/settings/siren-status` | Siren status configuration |
| `POST` | `/api/settings/import` | Import settings from `appsettings.json` |
| `GET` | `/api/backups` | List backups |
| `POST` | `/api/backups` | Create a manual backup |
| `POST` | `/api/backups/{id}/restore` | Restore a backup |
| `DELETE` | `/api/backups/{id}` | Delete a backup |

### Development
#### Prerequisites
- .NET 10.0 SDK
- Node.js 22+ with pnpm
- Git

#### Build, test, and publish
```bash
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet publish
```

`dotnet publish` also builds the Admin UI and copies the generated files into `wwwroot/`.

#### Admin UI (Nuxt 3)
```bash
cd TetraControl2Connect/AdminUI
pnpm install
pnpm run dev
pnpm run generate
pnpm test
```

During development, the Admin UI runs on `http://localhost:3000`. Requests to `/api` and `/hubs` are proxied to `http://localhost:5050`. `pnpm run generate` produces the static output in `.output/public/`.

### Simulator
```bash
dotnet run --project TetraControl2Connect.Simulator -- [port]
```

- Default port: `8085`
- WebSocket endpoint: `ws://localhost:{port}/live.json`
- Default credentials: `Connect / Connect`
- Before startup, you must confirm that no production Connect environment will be affected.
- The simulator offers interactive scenarios for fire alarms, traffic accidents, siren tests, user availability changes, and continuous random messages.

### Architecture and repository structure
- `TetraControl2Connect` – ASP.NET Core web application with the settings API, a SignalR hub at `/hubs/messages`, and the integrated Nuxt Admin UI
- `TetraControl2Connect/AdminUI` – Nuxt 3 frontend built with `@nuxt/ui`
- `TetraControl2Connect.Shared` – Shared option classes and models
- `TetraControl2Connect.Test` – xUnit test project
- `TetraControl2Connect.Simulator` – WebSocket simulator for TetraControl
- Configuration data is stored in `settings.db`; log files are written to the `logs/` directory.

### License
[GNU Affero General Public License v3.0](LICENSE)

Copyright Feuer Software GmbH  
Website: https://feuersoftware.com
