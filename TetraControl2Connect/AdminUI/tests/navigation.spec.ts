import { test, expect } from '@playwright/test'

test.describe('Navigation', () => {
  // Desktop sidebar selector (the mobile drawer is Teleported and may appear first in DOM)
  const desktopSidebar = 'aside.shrink-0'

  test('dashboard page loads with correct title', async ({ page }) => {
    await page.goto('/')
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()
    await expect(page.getByText('Übersicht aller Einstellungsbereiche')).toBeVisible()
  })

  test('sidebar shows all navigation items', async ({ page }) => {
    await page.goto('/')
    const sidebar = page.locator(desktopSidebar)
    await expect(sidebar.getByRole('heading', { name: 'TetraControl2Connect' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Dashboard' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Programmoptionen' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'TetraControl', exact: true })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Connect / Standorte' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Status', exact: true })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Muster (Pattern)' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Schweregrade' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Sirenen-Alarmierung' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Sirenen-Status' })).toBeVisible()
    await expect(sidebar.getByRole('link', { name: 'Live-Ansicht' })).toBeVisible()
  })

  test('navigate to Programmoptionen', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Programmoptionen' }).click()
    await expect(page).toHaveURL('/settings/program')
    await expect(page.getByRole('heading', { name: 'Programmoptionen', exact: true })).toBeVisible()
  })

  test('navigate to TetraControl', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'TetraControl', exact: true }).click()
    await expect(page).toHaveURL('/settings/tetracontrol')
    await expect(page.getByRole('heading', { name: 'TetraControl', exact: true })).toBeVisible()
  })

  test('navigate to Connect / Standorte', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Connect / Standorte' }).click()
    await expect(page).toHaveURL('/settings/connect')
    await expect(page.getByRole('heading', { name: 'Connect / Standorte' })).toBeVisible()
  })

  test('navigate to Status', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Status', exact: true }).click()
    await expect(page).toHaveURL('/settings/status')
    await expect(page.getByRole('heading', { name: 'Status', exact: true })).toBeVisible()
  })

  test('navigate to Muster (Pattern)', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Muster (Pattern)' }).click()
    await expect(page).toHaveURL('/settings/pattern')
    await expect(page.getByRole('heading', { name: 'Muster (Pattern)' })).toBeVisible()
  })

  test('navigate to Schweregrade', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Schweregrade' }).click()
    await expect(page).toHaveURL('/settings/severity')
    await expect(page.getByRole('heading', { name: 'Schweregrade' })).toBeVisible()
  })

  test('navigate to Sirenen-Alarmierung', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Sirenen-Alarmierung' }).click()
    await expect(page).toHaveURL('/settings/siren-callout')
    await expect(page.getByRole('heading', { name: 'Sirenen-Alarmierung' })).toBeVisible()
  })

  test('navigate to Sirenen-Status', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Sirenen-Status' }).click()
    await expect(page).toHaveURL('/settings/siren-status')
    await expect(page.getByRole('heading', { name: 'Sirenen-Status' })).toBeVisible()
  })

  test('navigate to Live-Ansicht', async ({ page }) => {
    await page.goto('/')
    await page.locator(desktopSidebar).getByRole('link', { name: 'Live-Ansicht' }).click()
    await expect(page).toHaveURL('/live')
    await expect(page.getByRole('heading', { name: 'Live-Ansicht' })).toBeVisible()
  })

  test('mobile responsive: sidebar toggle works', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 })
    await page.goto('/')
    await page.waitForLoadState('networkidle')

    // Desktop sidebar should be hidden on mobile
    await expect(page.locator(desktopSidebar)).toBeHidden()

    // Mobile header with toggle button should be visible
    const mobileHeader = page.locator('div.lg\\:hidden.fixed').first()
    await expect(mobileHeader).toBeVisible()
    await expect(mobileHeader.getByText('TetraControl2Connect Admin')).toBeVisible()

    // Click the menu toggle button to open drawer
    await mobileHeader.getByRole('button').click()

    // Mobile drawer aside should appear with TC2C Admin heading
    const drawerAside = page.locator('aside').filter({ hasText: 'TC2C Admin' })
    await expect(drawerAside).toBeVisible()

    // Navigate via mobile drawer
    await drawerAside.getByRole('link', { name: 'Programmoptionen' }).click()
    await expect(page).toHaveURL('/settings/program')
  })
})
