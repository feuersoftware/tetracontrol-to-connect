# TetraControl2Connect

[🇬🇧 English Version](readme.md)

## Beschreibung
Dient der Verbindung von mehreren Connect-Standorten (auch aus unterschiedlichen Organisationen) mit einer TetraControl-Instanz. Die Konfiguration erfolgt bequem über die integrierte Admin-Oberfläche.

## Voraussetzungen
* .NET 10.0 Runtime oder höher
* TetraControl muss durchgehend laufen (am besten auf localhost, WebSocket-Verbindung ist nicht verschlüsselt!)
* In TetraControl muss der Webserver aktiviert sein
* In TetraControl muss ein entsprechender Benutzer mit ausreichenden Berechtigungen für den Webserver angelegt werden
* Pro Standort wird ein API-Key der „Öffentlichen Connect-Schnittstelle" benötigt — diesen findest du im Connect-Portal unter https://connect.feuersoftware.com/v2/app/interfaces

## Hinweise
* Wenn Fahrzeuge oder Benutzer neu angelegt oder geändert werden, muss das Programm neu gestartet werden.
* Wir können nur Daten verarbeiten, wenn die ISSIs der Fahrzeuge und Benutzer in Connect korrekt hinterlegt sind.

## Installation & Ersteinrichtung

1. Anwendung in ein Verzeichnis entpacken
2. Programm starten — die Admin-Oberfläche öffnet sich automatisch unter `http://localhost:5050`
3. Beim ersten Start wird die Datenbank mit Standardwerten befüllt
4. Der Einrichtungsassistent unter `/setup` führt dich durch die wichtigsten Einstellungen

> **Upgrade von einer älteren Version:** Wenn eine bestehende `appsettings.json` mit Konfigurationsabschnitten vorhanden ist, werden die Einstellungen beim ersten Start automatisch in die Datenbank importiert. Die `appsettings.json` kann anschließend auf die Logging-Konfiguration reduziert werden.

## Admin-Oberfläche

Die Anwendung stellt unter `http://localhost:5050` eine Admin-Oberfläche bereit:

* **Dashboard** — Übersicht aller Einstellungsbereiche mit Schnellzugriff
* **Einrichtungsassistent** (`/setup`) — Schritt-für-Schritt-Konfiguration bei Ersteinrichtung
* **Einstellungsseiten** — Editor für alle 8 Konfigurationsabschnitte:
  * Programmoptionen — Steuerung der übertragenen Datenarten und Timeouts
  * TetraControl-Verbindung — Host, Port, Zugangsdaten
  * Connect-Standorte — API-Keys, Subnetzadressen, Sirenen
  * Status-Zuordnungen — Pager-Statuswerte für Verfügbarkeits- und Einsatzrückmeldungen
  * Muster (Pattern) — Regex-Muster für die SDS-Alarmauswertung
  * Schweregrade — Übersetzung der Alarmierungs-Schweregrade in Stichwörter
  * Sirenen-Alarmierung — Steuercode-Übersetzung für Sirenenalarme
  * Sirenen-Status — Fehlercode-Übersetzung für Sirenenüberwachung
* **Live-Ansicht** (`/live`) — Echtzeit-Nachrichten von TetraControl
* **Backups** (`/backups`) — Sicherung und Wiederherstellung der Konfiguration

## Datenbank

Alle Einstellungen werden in einer SQLite-Datenbank (`settings.db`) gespeichert und über EF Core Migrations verwaltet. Die Datenbank wird beim ersten Start automatisch erstellt und befüllt.

Es werden automatisch tägliche Backups der Konfiguration angelegt, die über die Admin-Oberfläche verwaltet werden können.

## Web API

Die REST API ist unter `http://localhost:5050/api/settings/` erreichbar:

| Methode | Pfad | Beschreibung |
|---------|------|--------------|
| `GET` | `/api/settings` | Übersicht aller Abschnitte |
| `GET/PUT` | `/api/settings/program` | Programmoptionen |
| `GET/PUT` | `/api/settings/tetracontrol` | TetraControl-Verbindung |
| `GET/PUT` | `/api/settings/connect` | Standorte, Subnetze, Sirenen |
| `GET/PUT` | `/api/settings/status` | Status-Zuordnungen |
| `GET/PUT` | `/api/settings/pattern` | Muster für Alarmauswertung |
| `GET/PUT` | `/api/settings/severity` | Schweregrad-Übersetzungen |
| `GET/PUT` | `/api/settings/siren-callout` | Sirenen-Alarmierung |
| `GET/PUT` | `/api/settings/siren-status` | Sirenen-Status |
| `POST` | `/api/settings/import` | Import aus appsettings.json |

Swagger UI: `http://localhost:5050/swagger`

## Funktionsweise
Das Programm verbindet sich über eine WebSocket-Verbindung mit dem Webserver von TetraControl und verarbeitet Positionsdaten, Statusdaten und SDS. Funkgespräche werden ignoriert.

## Entwicklung

### Voraussetzungen
* .NET 10.0 SDK
* Node.js 22+ mit pnpm
* Git

### Projekt bauen
```bash
dotnet build
```

### Tests ausführen
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
pnpm run dev          # Dev-Server auf :3000 (Proxy zu .NET auf :5050)
pnpm run generate     # SPA-Build → .output/public/
pnpm test             # Playwright E2E-Tests
```

Während der Entwicklung leitet der SPA-Proxy in der .NET-App Anfragen automatisch an den Nuxt Dev-Server weiter. Für die Veröffentlichung wird die Admin-UI per `dotnet publish` gebaut und als statische SPA aus `wwwroot/` ausgeliefert.

### Simulator
```bash
dotnet run --project TetraControl2Connect.Simulator -- [port]
```
Startet einen WebSocket-Server, der das TetraControl-Protokoll simuliert. Standardport: 8085.

## Architektur

### Projekte
* `TetraControl2Connect` — ASP.NET Core Web-App mit Hosted Services, Web API, SignalR und Admin UI (Nuxt 3 SPA)
* `TetraControl2Connect.Shared` — Gemeinsame Options-Klassen und Modelle
* `TetraControl2Connect.Test` — xUnit-Tests (95+)
* `TetraControl2Connect.Simulator` — TetraControl WebSocket-Simulator

### Datenhaltung
* **SQLite** (`settings.db`) — Relationale Tabellen für alle Einstellungen, verwaltet über EF Core Migrations
* **Automatische Erstbefüllung** — Beim ersten Start: Import aus bestehender `appsettings.json` oder Befüllung mit Standardwerten
* **Tägliche Backups** — Automatische Sicherung bei Konfigurationsänderungen

### SPA-Integration
* **Entwicklung:** `Microsoft.AspNetCore.SpaProxy` startet den Nuxt Dev-Server automatisch und leitet Anfragen weiter
* **Produktion:** `dotnet publish` baut die SPA und kopiert sie nach `wwwroot/`; ASP.NET Core liefert sie als statische Dateien aus

## Copyright
Copyright Feuer Software GmbH

Internet: https://feuersoftware.com

Alle Rechte vorbehalten.