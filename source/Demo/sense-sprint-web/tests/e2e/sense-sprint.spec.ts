import { test, expect, Page } from '@playwright/test'
import { mockLanguagesResponse } from './mocks'

const firstSenseSprintRound = {
  roundId: 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa',
  language: 'en',
  expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
  clueIndex: 0,
  clue: 'a building for human habitation',
  scoreIfCorrect: 4,
  maxClues: 4,
  roundSeconds: 60,
  rateLimit: null,
}

const secondSenseSprintRoundWithoutLanguage = {
  roundId: 'cccccccc-3333-3333-3333-cccccccccccc',
  expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
  clueIndex: 0,
  clue: 'a small domesticated carnivorous mammal',
  scoreIfCorrect: 4,
  maxClues: 4,
  roundSeconds: 60,
  rateLimit: null,
}

const wonGuessResponse = {
  roundId: firstSenseSprintRound.roundId,
  isCorrect: true,
  roundStatus: 'won',
  awardedPoints: 4,
  currentClueIndex: 0,
  correctAnswer: 'house',
  message: 'Correct!',
}

async function setupLanguagesMock(page: Page) {
  await page.route('/languages', (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockLanguagesResponse) }),
  )
}

test.describe('Sense Sprint — round lifecycle', () => {
  test('shows wrong answers entered after incorrect guesses', async ({ page }) => {
    await setupLanguagesMock(page)

    await page.route('/game/sense-sprint/rounds', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(firstSenseSprintRound),
      })
    })

    await page.route(/\/game\/sense-sprint\/rounds\/.*\/guess/, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          roundId: firstSenseSprintRound.roundId,
          isCorrect: false,
          roundStatus: 'in-progress',
          awardedPoints: 0,
          currentClueIndex: 0,
          correctAnswer: null,
          message: 'Not quite',
        }),
      })
    })

    await page.goto('/')

    await page.getByRole('button', { name: /Start Round/i }).click()
    await page.getByPlaceholder('Type your guess').fill('castle')
    await page.getByRole('button', { name: /Submit Guess/i }).click()

    await expect(page.locator('[role="status"]')).toContainText(/Not correct yet/i)
    await expect(page.locator('.wrong-history-list li')).toHaveCount(1)
    await expect(page.locator('.wrong-history-list')).toContainText('castle')
  })

  test('starting a new round after a win does not blank the screen when language is missing in the response', async ({ page }) => {
    const pageErrors: Error[] = []
    page.on('pageerror', (error) => {
      pageErrors.push(error)
    })

    await setupLanguagesMock(page)

    let createRoundCallCount = 0
    await page.route('/game/sense-sprint/rounds', async (route) => {
      createRoundCallCount += 1
      const payload = createRoundCallCount === 1 ? firstSenseSprintRound : secondSenseSprintRoundWithoutLanguage
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(payload),
      })
    })

    await page.route(/\/game\/sense-sprint\/rounds\/.*\/guess/, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(wonGuessResponse),
      })
    })

    await page.goto('/')

    await page.getByRole('button', { name: /Start Round/i }).click()
    await expect(page.locator('.clue-text')).toHaveText(firstSenseSprintRound.clue)

    await page.getByPlaceholder('Type your guess').fill('house')
    await page.getByRole('button', { name: /Submit Guess/i }).click()
    await expect(page.locator('[role="status"]')).toContainText(/Correct!/i)

    await page.getByRole('button', { name: /New Round/i }).click()

    await expect(page.locator('.clue-text')).toHaveText(secondSenseSprintRoundWithoutLanguage.clue)
    await expect(page.locator('[role="status"]')).toContainText(/Round started/i)
    await expect(page.locator('.footer')).toContainText(/Active language: EN/i)
    await expect(pageErrors).toHaveLength(0)
  })

  test('uses the selected language when creating a new round', async ({ page }) => {
    await setupLanguagesMock(page)

    await page.addInitScript(() => {
      window.localStorage.removeItem('sense-sprint-session-v1')
      window.localStorage.removeItem('sense-sprint-stats-v1')
    })

    let requestedLanguage: string | undefined
    await page.route('/game/sense-sprint/rounds', async (route) => {
      const postData = route.request().postDataJSON() as { language?: string } | null
      requestedLanguage = postData?.language

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          roundId: 'dddddddd-4444-4444-4444-dddddddddddd',
          language: 'de',
          expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
          clueIndex: 0,
          clue: 'ein Gebaude zum Wohnen',
          scoreIfCorrect: 4,
          maxClues: 4,
          roundSeconds: 60,
          rateLimit: null,
        }),
      })
    })

    await page.goto('/')

    await page.locator('#language-select').selectOption('de')
    await page.getByRole('button', { name: /Start Round/i }).click()

    expect(requestedLanguage).toBe('de')
    await expect(page.locator('.clue-text')).toHaveText('ein Gebaude zum Wohnen')
    await expect(page.locator('.footer')).toContainText(/Active language: DE/i)
  })

  test('timer expiry with round-not-found response is handled once without repeated give-up calls', async ({ page }) => {
    const pageErrors: Error[] = []
    page.on('pageerror', (error) => {
      pageErrors.push(error)
    })

    await setupLanguagesMock(page)

    await page.addInitScript(() => {
      window.localStorage.removeItem('sense-sprint-session-v1')
      window.localStorage.removeItem('sense-sprint-stats-v1')
    })

    await page.route('/game/sense-sprint/rounds', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          roundId: 'eeeeeeee-5555-5555-5555-eeeeeeeeeeee',
          language: 'en',
          expiresAtUtc: new Date(Date.now() + 1_000).toISOString(),
          clueIndex: 0,
          clue: 'a period of sixty seconds',
          scoreIfCorrect: 4,
          maxClues: 4,
          roundSeconds: 1,
          rateLimit: null,
        }),
      })
    })

    let giveUpCallCount = 0
    await page.route(/\/game\/sense-sprint\/rounds\/.*\/give-up/, async (route) => {
      giveUpCallCount += 1
      await route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({
          title: 'Round not found',
          detail: 'Round not found. Start a new round.',
        }),
      })
    })

    await page.goto('/')

    await page.getByRole('button', { name: /Start Round/i }).click()
    await expect(page.locator('.clue-text')).toHaveText('a period of sixty seconds')

    await expect(page.locator('[role="status"]')).toContainText(/Time expired/i, { timeout: 8_000 })
    await page.waitForTimeout(1_500)

    expect(giveUpCallCount).toBe(1)
    await expect(pageErrors).toHaveLength(0)
  })
})