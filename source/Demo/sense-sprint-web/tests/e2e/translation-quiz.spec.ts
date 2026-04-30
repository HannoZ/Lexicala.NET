import { test, expect, Page } from '@playwright/test'
import {
  mockLanguagesResponse,
  mockQuizRound,
  mockQuizCorrectAnswer,
  mockQuizWrongAnswer,
  mockQuizExpired,
} from './mocks'

// ─── helpers ─────────────────────────────────────────────────────────────────

async function setupLanguagesMock(page: Page) {
  await page.route('/languages', (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockLanguagesResponse) }),
  )
}

async function setupQuizRoundMock(page: Page) {
  await page.route('/game/translation-quiz/rounds', (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockQuizRound) }),
  )
}

async function setupAnswerMock(page: Page, response: object) {
  await page.route(/\/game\/translation-quiz\/rounds\/.*\/answer/, (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(response) }),
  )
}

async function setupExpireMock(page: Page, response: object) {
  await page.route(/\/game\/translation-quiz\/rounds\/.*\/expire/, (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(response) }),
  )
}

async function openTranslationQuizTab(page: Page) {
  await page.getByRole('tab', { name: 'Translation Quiz' }).click()
}

// ─── tests ────────────────────────────────────────────────────────────────────

test.describe('Translation Quiz — language dropdown', () => {
  test('shows "Random (any language)" as first option', async ({ page }) => {
    await setupLanguagesMock(page)
    await page.goto('/')

    await openTranslationQuizTab(page)

    const select = page.locator('#quiz-target-language')
    await expect(select).toBeVisible()

    // First option should be the random placeholder with empty value
    const firstOption = select.locator('option').first()
    await expect(firstOption).toHaveText(/Random/i)
    await expect(firstOption).toHaveAttribute('value', '')
  })

  test('populates dropdown with languages from the API', async ({ page }) => {
    await setupLanguagesMock(page)
    await page.goto('/')

    await openTranslationQuizTab(page)

    // All five API target languages should appear in the dropdown
    const select = page.locator('#quiz-target-language')
    for (const [code, name] of [['de', 'German'], ['nl', 'Dutch'], ['fr', 'French'], ['es', 'Spanish'], ['ja', 'Japanese']]) {
      const option = select.locator(`option[value="${code}"]`)
      await expect(option).toHaveText(new RegExp(name, 'i'))
    }
  })

  test('falls back gracefully when languages API fails', async ({ page }) => {
    await page.route('/languages', (route) => route.fulfill({ status: 500 }))
    await page.goto('/')

    await openTranslationQuizTab(page)

    // Dropdown still visible with at least the random option
    const select = page.locator('#quiz-target-language')
    await expect(select).toBeVisible()
    await expect(select.locator('option').first()).toHaveText(/Random/i)
  })
})

test.describe('Translation Quiz — round lifecycle', () => {
  test.beforeEach(async ({ page }) => {
    await setupLanguagesMock(page)
    await setupQuizRoundMock(page)
  })

  test('starts a round and shows the source word and four choices', async ({ page }) => {
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()

    await expect(page.getByText('house')).toBeVisible()

    const choices = page.locator('.quiz-choice')
    await expect(choices).toHaveCount(4)
    for (const choice of ['Haus', 'Auto', 'Baum', 'Buch']) {
      await expect(page.getByRole('button', { name: choice })).toBeVisible()
    }
  })

  test('shows the target language name in the quiz prompt', async ({ page }) => {
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()

    // "de" → "German" is resolved from the language names in the languages API
    await expect(page.locator('.quiz-word-label')).toContainText(/German/i)
  })

  test('correct choice highlights green and shows won status', async ({ page }) => {
    await setupAnswerMock(page, mockQuizCorrectAnswer)
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()
    await page.getByRole('button', { name: 'Haus' }).click()

    await expect(page.getByRole('button', { name: 'Haus' })).toHaveClass(/quiz-choice-correct/)
    await expect(page.locator('[role="status"]')).toContainText(/Correct/i)
  })

  test('wrong choice highlights red and shows the correct answer', async ({ page }) => {
    await setupAnswerMock(page, mockQuizWrongAnswer)
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()
    await page.getByRole('button', { name: 'Auto' }).click()

    await expect(page.getByRole('button', { name: 'Auto' })).toHaveClass(/quiz-choice-wrong/)
    await expect(page.getByRole('button', { name: 'Haus' })).toHaveClass(/quiz-choice-correct/)
    await expect(page.locator('[role="status"]')).toContainText(/Haus/i)
  })

  test('choices are disabled after an answer is submitted', async ({ page }) => {
    await setupAnswerMock(page, mockQuizCorrectAnswer)
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()
    await page.getByRole('button', { name: 'Haus' }).click()

    for (const choice of ['Haus', 'Auto', 'Baum', 'Buch']) {
      await expect(page.getByRole('button', { name: choice })).toBeDisabled()
    }
  })

  test('"New Round" button appears after completing a round', async ({ page }) => {
    await setupAnswerMock(page, mockQuizCorrectAnswer)
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()
    await page.getByRole('button', { name: 'Haus' }).click()

    await expect(page.getByRole('button', { name: /New Round/i })).toBeVisible()
  })
})

test.describe('Translation Quiz — stats', () => {
  test.beforeEach(async ({ page }) => {
    await setupLanguagesMock(page)
    await setupQuizRoundMock(page)
    // Clear any persisted quiz stats from prior test runs
    await page.addInitScript(() => {
      window.localStorage.removeItem('translation-quiz-stats-v1')
    })
  })

  test('win streak increments on correct answer', async ({ page }) => {
    await setupAnswerMock(page, mockQuizCorrectAnswer)
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()
    await page.getByRole('button', { name: 'Haus' }).click()

    await expect(page.getByText('1').first()).toBeVisible()
    await expect(page.locator('.kpi', { hasText: /Win Streak/i })).toContainText('1')
  })

  test('win streak resets to 0 on wrong answer', async ({ page }) => {
    await setupAnswerMock(page, mockQuizWrongAnswer)
    await page.goto('/')
    await openTranslationQuizTab(page)

    await page.getByRole('button', { name: /Start Round/i }).click()
    await page.getByRole('button', { name: 'Auto' }).click()

    await expect(page.locator('.kpi', { hasText: /Win Streak/i })).toContainText('0')
  })

  test('rounds played counter increments on new round', async ({ page }) => {
    await page.goto('/')
    await openTranslationQuizTab(page)

    await expect(page.locator('.kpi', { hasText: /Rounds/i })).toContainText('0')
    await page.getByRole('button', { name: /Start Round/i }).click()
    await expect(page.locator('.kpi', { hasText: /Rounds/i })).toContainText('1')
  })
})

test.describe('Translation Quiz — timer expiry', () => {
  test('reveals the correct answer via /expire when timer reaches 0', async ({ page }) => {
    await setupLanguagesMock(page)
    // Create the mock lazily so the expiry time is computed when the request arrives
    await page.route('/game/translation-quiz/rounds', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...mockQuizRound,
          expiresAtUtc: new Date(Date.now() + 2_000).toISOString(),
          roundSeconds: 2,
        }),
      }),
    )
    await setupExpireMock(page, mockQuizExpired)

    await page.goto('/')
    await openTranslationQuizTab(page)
    await page.getByRole('button', { name: /Start Round/i }).click()

    // Allow a small tolerance for client-side timer rounding; reject if called well before expiry
    await expect(page.locator('[role="status"]')).toContainText(/Time.s up/i, { timeout: 5_000 })
    await expect(page.getByRole('button', { name: 'Haus' })).toHaveClass(/quiz-choice-correct/, { timeout: 5_000 })
  })
})
