import { test, expect } from '@playwright/test'

test.describe('Settings Pages', () => {
  test.describe('Program Settings', () => {
    test('shows toggle switches and input fields', async ({ page }) => {
      await page.goto('/settings/program')
      await expect(page.getByRole('heading', { name: 'Programmoptionen' })).toBeVisible()
      await expect(page.getByText('Allgemeine Programmeinstellungen')).toBeVisible()

      // Boolean toggle fields
      await expect(page.getByText('Fahrzeugstatus senden')).toBeVisible()
      await expect(page.getByText('Fahrzeugpositionen senden')).toBeVisible()
      await expect(page.getByText('Alarme senden')).toBeVisible()
      await expect(page.getByText('Status 5 ignorieren')).toBeVisible()

      // Number input fields
      await expect(page.getByText('Verfügbarkeit Lebensdauer (Tage)')).toBeVisible()
      await expect(page.getByText('WebSocket Reconnect Timeout (Min.)')).toBeVisible()

      // URL/timespan fields
      await expect(page.getByText('Heartbeat Endpoint URL')).toBeVisible()
      await expect(page.getByText('Heartbeat-Intervall (HH:MM:SS)')).toBeVisible()

      // Save button
      await expect(page.getByRole('button', { name: 'Speichern' }).first()).toBeVisible()
    })
  })

  test.describe('TetraControl Settings', () => {
    test('shows host, port, username, password fields', async ({ page }) => {
      await page.goto('/settings/tetracontrol')
      await expect(page.getByRole('heading', { name: 'TetraControl', exact: true })).toBeVisible()
      await expect(page.getByText('Verbindungseinstellungen zum TetraControl-Server')).toBeVisible()

      await expect(page.getByText('Host', { exact: true })).toBeVisible()
      await expect(page.getByText('Port', { exact: true })).toBeVisible()
      await expect(page.getByText('Benutzername', { exact: true })).toBeVisible()
      await expect(page.getByText('Passwort', { exact: true })).toBeVisible()

      // Password visibility toggle
      await expect(page.getByPlaceholder('Passwort')).toBeVisible()
      await expect(page.getByPlaceholder('z.B. 192.168.1.100')).toBeVisible()

      await expect(page.getByRole('button', { name: 'Speichern' }).first()).toBeVisible()
    })
  })

  test.describe('Connect Settings', () => {
    test('shows "Standort hinzufügen" button', async ({ page }) => {
      await page.goto('/settings/connect')
      await expect(page.getByRole('heading', { name: 'Connect / Standorte' })).toBeVisible()
      await expect(page.getByText('Standorte, Subnetzadressen und Sirenen verwalten')).toBeVisible()

      // The "Standort hinzufügen" button should be visible (in header or empty state)
      await expect(page.getByRole('button', { name: 'Standort hinzufügen', exact: true })).toBeVisible()
      await expect(page.getByRole('button', { name: 'Speichern' })).toBeVisible()
    })
  })

  test.describe('Status Settings', () => {
    test('shows all 6 status fields', async ({ page }) => {
      await page.goto('/settings/status')
      await expect(page.getByRole('heading', { name: 'Status', exact: true })).toBeVisible()
      await expect(page.getByText('Status-Zuordnungen für Einsatzkräfte')).toBeVisible()

      await expect(page.getByText('Verfügbar', { exact: true })).toBeVisible()
      await expect(page.getByText('Eingeschränkt verfügbar', { exact: true })).toBeVisible()
      await expect(page.getByText('Nicht verfügbar', { exact: true })).toBeVisible()
      await expect(page.getByText('Kommt', { exact: true })).toBeVisible()
      await expect(page.getByText('Kommt nicht', { exact: true })).toBeVisible()
      await expect(page.getByText('Kommt später', { exact: true })).toBeVisible()

      await expect(page.getByRole('button', { name: 'Speichern' }).first()).toBeVisible()
    })
  })

  test.describe('Pattern Settings', () => {
    test('shows pattern input fields', async ({ page }) => {
      await page.goto('/settings/pattern')
      await expect(page.getByRole('heading', { name: 'Muster (Pattern)' })).toBeVisible()
      await expect(page.getByText('Regex-Muster für die Alarm-Auswertung')).toBeVisible()

      // Standard pattern fields
      await expect(page.getByText('Einsatznummer')).toBeVisible()
      await expect(page.getByText('Stichwort', { exact: true })).toBeVisible()
      await expect(page.getByText('Sachverhalt')).toBeVisible()
      await expect(page.getByText('Stadt', { exact: true })).toBeVisible()
      await expect(page.getByText('Straße', { exact: true })).toBeVisible()
      await expect(page.getByText('PLZ')).toBeVisible()
      await expect(page.getByText('Breitengrad')).toBeVisible()
      await expect(page.getByText('Längengrad')).toBeVisible()
      await expect(page.getByText('RIC')).toBeVisible()

      // Additional properties section
      await expect(page.getByText('Zusätzliche Eigenschaften')).toBeVisible()
      await expect(page.getByRole('button', { name: 'Hinzufügen' })).toBeVisible()

      await expect(page.getByRole('button', { name: 'Speichern' }).first()).toBeVisible()
    })
  })

  test.describe('Severity Settings', () => {
    test('shows dictionary editor', async ({ page }) => {
      await page.goto('/settings/severity')
      await expect(page.getByRole('heading', { name: 'Schweregrade' })).toBeVisible()
      await expect(page.getByText('Schweregrad-Übersetzungen konfigurieren')).toBeVisible()

      // Options section with toggle
      await expect(page.getByText('Schweregrad-Übersetzung als Stichwort verwenden')).toBeVisible()

      // Translation section heading
      await expect(page.getByRole('heading', { name: 'Übersetzungen' })).toBeVisible()

      // New entry inputs
      await expect(page.getByPlaceholder('Neuer Schlüssel')).toBeVisible()
      await expect(page.getByPlaceholder('Übersetzung').first()).toBeVisible()

      await expect(page.getByRole('button', { name: 'Speichern' }).first()).toBeVisible()
    })
  })

  test.describe('Siren-Callout Settings', () => {
    test('shows dictionary editor', async ({ page }) => {
      await page.goto('/settings/siren-callout')
      await expect(page.getByRole('heading', { name: 'Sirenen-Alarmierung' })).toBeVisible()
      await expect(page.getByText('Sirenen-Code Übersetzungen konfigurieren')).toBeVisible()

      // Options section with toggle
      await expect(page.getByText('Sirenen-Code Übersetzung als Stichwort verwenden')).toBeVisible()

      // Translation section
      await expect(page.getByRole('heading', { name: 'Sirenen-Code Übersetzungen' })).toBeVisible()

      // New entry inputs
      await expect(page.getByPlaceholder('Neuer Code')).toBeVisible()
      await expect(page.getByPlaceholder('Übersetzung').first()).toBeVisible()

      await expect(page.getByRole('button', { name: 'Speichern' }).first()).toBeVisible()
    })
  })

  test.describe('Siren-Status Settings', () => {
    test('shows dictionary editor', async ({ page }) => {
      await page.goto('/settings/siren-status')
      await expect(page.getByRole('heading', { name: 'Sirenen-Status' })).toBeVisible()
      await expect(page.getByText('Sirenen Fehler-Übersetzungen konfigurieren')).toBeVisible()

      // Translation section heading
      await expect(page.getByRole('heading', { name: 'Fehler-Übersetzungen' })).toBeVisible()

      // New entry inputs
      await expect(page.getByPlaceholder('Neuer Fehler-Code')).toBeVisible()
      await expect(page.getByPlaceholder('Übersetzung').first()).toBeVisible()

      await expect(page.getByRole('button', { name: 'Speichern' }).first()).toBeVisible()
    })
  })
})
