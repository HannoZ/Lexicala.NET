/**
 * Shared mock responses used by the E2E tests so that all tests run without a
 * live Lexicala backend.  Every route intercept is set up via Playwright's
 * `page.route()` API before the page is loaded.
 */

export const mockLanguagesResponse = {
  language_names: {
    en: 'English',
    de: 'German',
    nl: 'Dutch',
    fr: 'French',
    es: 'Spanish',
    ja: 'Japanese',
  },
  resources: {
    global: {
      source_languages: ['en', 'de', 'nl', 'fr', 'es', 'ja'],
      target_languages: ['de', 'nl', 'fr', 'es', 'ja'],
    },
  },
}

export const mockSenseSprintRound = {
  roundId: 'aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa',
  language: 'en',
  expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
  clueIndex: 0,
  clue: 'a building for human habitation',
  scoreIfCorrect: 10,
  maxClues: 5,
  roundSeconds: 60,
  rateLimit: null,
}

export const mockQuizRound = {
  roundId: 'bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb',
  sourceWord: 'house',
  sourceLanguage: 'en',
  targetLanguage: 'de',
  choices: ['Haus', 'Auto', 'Baum', 'Buch'],
  expiresAtUtc: new Date(Date.now() + 30_000).toISOString(),
  roundSeconds: 30,
  rateLimit: null,
}

export const mockQuizCorrectAnswer = {
  roundId: 'bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb',
  isCorrect: true,
  roundStatus: 'won',
  awardedPoints: 1,
  correctAnswer: 'Haus',
  message: 'Correct!',
}

export const mockQuizWrongAnswer = {
  roundId: 'bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb',
  isCorrect: false,
  roundStatus: 'lost',
  awardedPoints: 0,
  correctAnswer: 'Haus',
  message: 'Incorrect. The correct translation was: Haus',
}

export const mockQuizExpired = {
  roundId: 'bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb',
  isCorrect: false,
  roundStatus: 'expired',
  awardedPoints: 0,
  correctAnswer: 'Haus',
  message: "Time's up!",
}
