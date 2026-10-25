import { test, expect } from '@playwright/test'

const backups = [
  { id: 3, createdAt: '2026-10-25T08:00:00+00:00', description: 'automatic_before_import' },
  { id: 2, createdAt: '2026-10-24T08:00:00+00:00', description: 'manual' },
  { id: 1, createdAt: '2026-10-23T08:00:00+00:00', description: 'automatic' }
]

test('backup description codes are translated', async ({ page, context, baseURL }) => {
  await page.route('**/api/backups', route => route.fulfill({ json: backups }))

  await page.goto('/backups')
  await expect(page.getByText('Automatische Sicherung vor Import', { exact: true })).toBeVisible()
  await expect(page.getByText('Manuelle Sicherung', { exact: true })).toBeVisible()
  await expect(page.getByText('Automatische Sicherung', { exact: true })).toBeVisible()
  await expect(page.getByText('automatic_before_import', { exact: true })).toHaveCount(0)

  await context.addCookies([{ name: 'i18n_locale', value: 'en', url: baseURL! }])
  await page.reload()
  await expect(page.getByText('Automatic backup before import', { exact: true })).toBeVisible()
  await expect(page.getByText('Manual backup', { exact: true })).toBeVisible()
  await expect(page.getByText('Automatic backup', { exact: true })).toBeVisible()
})

test('backup errors show translated text instead of the API message', async ({ page }) => {
  await page.route('**/api/backups', route => route.fulfill({
    status: 500, json: { message: 'SQLite Error 1: no such table.' }
  }))

  await page.goto('/backups')
  await expect(page.getByText('Sicherungen konnten nicht geladen werden.', { exact: true })).toBeVisible()
  await expect(page.getByText('SQLite Error', { exact: false })).toHaveCount(0)
})
