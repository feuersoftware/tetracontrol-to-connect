import { test, expect } from '@playwright/test'

test.describe('Live View', () => {
  test('page loads with waiting message', async ({ page }) => {
    await page.goto('/live')
    await expect(page.getByRole('heading', { name: 'Live-Ansicht' })).toBeVisible()
    await expect(page.getByText('Echtzeit-Nachrichten von TetraControl')).toBeVisible()
    await expect(page.getByText('Warte auf Nachrichten...')).toBeVisible()
  })

  test('connection status indicators are visible', async ({ page }) => {
    await page.goto('/live')

    // Live connection status
    await expect(page.getByText('Live-Verbindung getrennt')).toBeVisible()

    // TetraControl connection status (in the main content area, not sidebar)
    await expect(page.getByText('TetraControl getrennt')).toBeVisible()

    // Status indicator dots (both should show disconnected since no backend)
    const dots = page.locator('.rounded-full')
    await expect(dots.first()).toBeVisible()
  })

  test('"Leeren" button is visible', async ({ page }) => {
    await page.goto('/live')
    await expect(page.getByRole('button', { name: 'Leeren' })).toBeVisible()
  })
})
