import { test, expect } from '@playwright/test'

test('dashboard warns only when no sites exist', async ({ page }) => {
  let hasSites = false
  await page.route('**/api/settings', route => route.fulfill({
    json: { sections: [{ section: 'tetracontrol', exists: true }, { section: 'connect', exists: hasSites }] }
  }))
  await page.route('**/api/update', route => route.fulfill({ json: { hasUpdate: false } }))

  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Keine Standorte konfiguriert' })).toBeVisible()
  await expect(page.getByRole('link', { name: 'Konfiguration importieren' })).toHaveAttribute('href', '/backups')

  hasSites = true
  await page.reload()
  await expect(page.getByText('Konfiguriert', { exact: true }).first()).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Keine Standorte konfiguriert' })).toHaveCount(0)
})

test('import requires a file and confirmation, then uploads its contents', async ({ page }) => {
  await page.route('**/api/backups', route => route.fulfill({ json: [] }))
  const json = '{"ConnectOptions":{"Sites":[{"Name":"Teststandort","Key":"test-value"}]}}'
  let imported = false
  await page.route('**/api/settings/import', async route => {
    expect(route.request().method()).toBe('POST')
    expect(route.request().postData()).toBe(json)
    imported = true
    await route.fulfill({ json: { message: 'Importiert' } })
  })

  await page.goto('/backups')
  const button = page.getByRole('button', { name: 'Datei importieren', exact: true })
  await expect(button).toBeDisabled()
  await page.getByLabel('appsettings.json auswählen').setInputFiles({
    name: 'appsettings.json', mimeType: 'application/json', buffer: Buffer.from(json)
  })
  await expect(button).toBeDisabled()
  await page.getByRole('checkbox').check()
  await button.click()
  await expect(page.getByText('Einstellungen importiert', { exact: true })).toBeVisible()
  expect(imported).toBe(true)
  await expect(button).toBeDisabled()
})

test('empty files are rejected without calling the import', async ({ page }) => {
  await page.route('**/api/backups', route => route.fulfill({ json: [] }))
  let called = false
  await page.route('**/api/settings/import', async route => {
    called = true
    await route.fulfill({ json: { message: 'Importiert' } })
  })
  await page.goto('/backups')
  await page.getByLabel('appsettings.json auswählen').setInputFiles({
    name: 'appsettings.json', mimeType: 'application/json', buffer: Buffer.from('')
  })
  await page.getByRole('checkbox').check()
  await page.getByRole('button', { name: 'Datei importieren', exact: true }).click()
  await expect(page.getByText('Die ausgewählte Datei ist leer.', { exact: false })).toBeVisible()
  await expect(page.getByText('Einstellungen importiert', { exact: true })).toHaveCount(0)
  expect(called).toBe(false)
})

test('import errors are shown without reporting success', async ({ page }) => {
  await page.route('**/api/backups', route => route.fulfill({ json: [] }))
  await page.route('**/api/settings/import', route => route.fulfill({
    status: 400, json: { message: 'Ungültige appsettings.json. Einstellungen unverändert.' }
  }))
  await page.goto('/backups')
  await page.getByLabel('appsettings.json auswählen').setInputFiles({
    name: 'appsettings.json', mimeType: 'application/json', buffer: Buffer.from('invalid')
  })
  await page.getByRole('checkbox').check()
  await page.getByRole('button', { name: 'Datei importieren', exact: true }).click()
  await expect(page.getByText('Import fehlgeschlagen', { exact: true })).toBeVisible()
  await expect(page.getByText('Einstellungen importiert', { exact: true })).toHaveCount(0)
})
