import { useEffect, useMemo, useRef, useState } from 'react'
import './App.css'

// ─── Shared types ─────────────────────────────────────────────────────────────

type GameMode = 'sense-sprint' | 'translation-quiz'
type RoundStatus = 'in-progress' | 'won' | 'lost' | 'expired' | 'completed'
type FeedbackTone = 'info' | 'success' | 'warning' | 'error'
type CelebrationLevel = 'none' | 'win' | 'major'

type RateLimitDebug = {
  limit: number
  limitRemaining: number
  resetSeconds: number
}

type ActiveRound = {
  roundId: string
  language: string
  clue: string
  clueIndex: number
  maxClues: number
  expiresAtUtc: string
  scoreIfCorrect: number
  roundStatus: RoundStatus
  answer: string | null
}

type CreateRoundResponse = {
  roundId: string
  language: string
  expiresAtUtc: string
  clueIndex: number
  clue: string
  scoreIfCorrect: number
  maxClues: number
  roundSeconds: number
  rateLimit: RateLimitDebug | null
}

type NextClueResponse = {
  roundId: string
  expiresAtUtc: string
  clueIndex: number
  clue: string
  scoreIfCorrect: number
  maxClues: number
}

type GuessResponse = {
  roundId: string
  isCorrect: boolean
  roundStatus: RoundStatus
  awardedPoints: number
  currentClueIndex: number
  correctAnswer: string | null
  message: string | null
}

type ProblemResponse = {
  title?: string
  detail?: string
}

type ApiResult<T> = {
  data: T
  rateLimit: RateLimitDebug | null
}

type LanguagesResponse = {
  language_names: Record<string, string>
  resources: {
    global?: {
      source_languages?: string[]
      target_languages?: string[]
    }
  }
}

type LanguageOption = {
  code: string
  name: string
}

type GameStats = {
  currentScore: number
  highScore: number
  roundsStarted: number
  roundsWon: number
  roundsLost: number
  roundsExpired: number
  roundsGivenUp: number
  roundsAbandoned: number
  wordsGuessedCorrectly: number
  cluesRequested: number
  totalCluesSeen: number
  totalGuessesSubmitted: number
  currentStreak: number
  highestStreak: number
  majorCelebrations: number
  lastPlayedAt: string | null
}

type StoredSession = {
  round: ActiveRound
  revealedClues: string[]
  wrongGuesses: string[]
  guess: string
}

// ─── Translation Quiz types ───────────────────────────────────────────────────

type ActiveQuizRound = {
  roundId: string
  sourceWord: string
  sourceLanguage: string
  targetLanguage: string
  choices: string[]
  expiresAtUtc: string
  roundStatus: RoundStatus
  correctAnswer: string | null
  selectedChoice: string | null
}

type CreateQuizRoundResponse = {
  roundId: string
  sourceWord: string
  sourceLanguage: string
  targetLanguage: string
  choices: string[]
  expiresAtUtc: string
  roundSeconds: number
  rateLimit: RateLimitDebug | null
}

type QuizAnswerResponse = {
  roundId: string
  isCorrect: boolean
  roundStatus: RoundStatus
  awardedPoints: number
  correctAnswer: string
  message: string | null
}

type QuizStats = {
  currentScore: number
  highScore: number
  roundsPlayed: number
  roundsWon: number
  currentStreak: number
  highestStreak: number
}

const statsStorageKey = 'sense-sprint-stats-v1'
const sessionStorageKey = 'sense-sprint-session-v1'
const quizStatsStorageKey = 'translation-quiz-stats-v1'

const defaultStats: GameStats = {
  currentScore: 0,
  highScore: 0,
  roundsStarted: 0,
  roundsWon: 0,
  roundsLost: 0,
  roundsExpired: 0,
  roundsGivenUp: 0,
  roundsAbandoned: 0,
  wordsGuessedCorrectly: 0,
  cluesRequested: 0,
  totalCluesSeen: 0,
  totalGuessesSubmitted: 0,
  currentStreak: 0,
  highestStreak: 0,
  majorCelebrations: 0,
  lastPlayedAt: null,
}

function loadStats(): GameStats {
  if (typeof window === 'undefined') {
    return { ...defaultStats }
  }

  const raw = window.localStorage.getItem(statsStorageKey)
  if (!raw) {
    return { ...defaultStats }
  }

  try {
    const parsed = JSON.parse(raw) as Partial<GameStats>
    return {
      ...defaultStats,
      ...parsed,
      lastPlayedAt: typeof parsed.lastPlayedAt === 'string' ? parsed.lastPlayedAt : null,
    }
  } catch {
    return { ...defaultStats }
  }
}

function isValidStoredRound(value: unknown): value is ActiveRound {
  if (!value || typeof value !== 'object') {
    return false
  }

  const candidate = value as Partial<ActiveRound>

  return (
    typeof candidate.roundId === 'string' &&
    typeof candidate.language === 'string' &&
    typeof candidate.clue === 'string' &&
    typeof candidate.clueIndex === 'number' &&
    typeof candidate.maxClues === 'number' &&
    typeof candidate.expiresAtUtc === 'string' &&
    typeof candidate.scoreIfCorrect === 'number' &&
    typeof candidate.roundStatus === 'string' &&
    (typeof candidate.answer === 'string' || candidate.answer === null)
  )
}

// ─── Translation Quiz stats helpers ──────────────────────────────────────────

const defaultQuizStats: QuizStats = {
  currentScore: 0,
  highScore: 0,
  roundsPlayed: 0,
  roundsWon: 0,
  currentStreak: 0,
  highestStreak: 0,
}

function loadQuizStats(): QuizStats {
  if (typeof window === 'undefined') return { ...defaultQuizStats }
  const raw = window.localStorage.getItem(quizStatsStorageKey)
  if (!raw) return { ...defaultQuizStats }
  try {
    return { ...defaultQuizStats, ...(JSON.parse(raw) as Partial<QuizStats>) }
  } catch {
    return { ...defaultQuizStats }
  }
}

function saveQuizStats(stats: QuizStats): void {
  if (typeof window !== 'undefined') {
    window.localStorage.setItem(quizStatsStorageKey, JSON.stringify(stats))
  }
}

function loadStoredSession(): StoredSession | null {
  if (typeof window === 'undefined') {
    return null
  }

  const raw = window.localStorage.getItem(sessionStorageKey)
  if (!raw) {
    return null
  }

  try {
    const parsed = JSON.parse(raw) as Partial<StoredSession>
    if (!isValidStoredRound(parsed.round) || parsed.round.roundStatus !== 'in-progress') {
      return null
    }

    return {
      round: parsed.round,
      revealedClues: Array.isArray(parsed.revealedClues)
        ? parsed.revealedClues.filter((item): item is string => typeof item === 'string')
        : [parsed.round.clue],
      wrongGuesses: Array.isArray(parsed.wrongGuesses)
        ? parsed.wrongGuesses.filter((item): item is string => typeof item === 'string')
        : [],
      guess: typeof parsed.guess === 'string' ? parsed.guess : '',
    }
  } catch {
    return null
  }
}

function clearStoredSession(): void {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.removeItem(sessionStorageKey)
}

function getSecondsRemaining(expiresAtUtc: string): number {
  return Math.max(0, Math.ceil((new Date(expiresAtUtc).getTime() - Date.now()) / 1000))
}

function getStreakMultiplier(streak: number): number {
  if (streak >= 15) {
    return 2
  }

  if (streak >= 10) {
    return 1.65
  }

  if (streak >= 5) {
    return 1.35
  }

  if (streak >= 3) {
    return 1.15
  }

  return 1
}

function getAdjustedScore(baseScore: number, streak: number): number {
  return Math.max(baseScore, Math.round(baseScore * getStreakMultiplier(streak)))
}

function getCelebrationLevel(streak: number): CelebrationLevel {
  if (streak > 0 && streak % 5 === 0) {
    return 'major'
  }

  if (streak > 0) {
    return 'win'
  }

  return 'none'
}

function formatAverage(value: number): string {
  if (!Number.isFinite(value)) {
    return '0.0'
  }

  return value.toFixed(1)
}

// ─── Languages Caching (Tier 1: ETag + localStorage) ──────────────────────────
// Caches the /languages response to eliminate redundant API calls
// Strategy: Check session cache → localStorage (with ETag) → API fallback

type CachedLanguages = {
  data: LanguagesResponse
  etag?: string
  timestamp: number
}

let sessionLanguagesCache: CachedLanguages | null = null

async function loadLanguagesWithCaching(): Promise<LanguagesResponse> {
  // 1️⃣ Check session-level cache (fastest, zero API calls if hit)
  if (sessionLanguagesCache) {
    return sessionLanguagesCache.data
  }

  // 2️⃣ Check localStorage (fast, but may need ETag refresh)
  let storedCache: CachedLanguages | null = null
  let headers: HeadersInit = {}

  if (typeof window !== 'undefined') {
    const stored = window.localStorage.getItem('lexicala-languages-cache-v1')
    if (stored) {
      try {
        storedCache = JSON.parse(stored) as CachedLanguages
        // If we have ETag from previous fetch, use it for conditional request
        if (storedCache.etag) {
          headers['If-None-Match'] = storedCache.etag
        }
      } catch {
        // Invalid cached data, clear it
        window.localStorage.removeItem('lexicala-languages-cache-v1')
        window.localStorage.removeItem('lexicala-languages-etag-v1')
      }
    }
  }

  // 3️⃣ Fetch from API (with ETag for conditional request)
  try {
    const response = await fetch('/languages', {
      headers: Object.keys(headers).length > 0 ? headers : undefined,
    })

    let result: LanguagesResponse

    if (response.status === 304 && storedCache) {
      // 304 Not Modified: Use cached data, content hasn't changed; preserve existing ETag
      result = storedCache.data
      sessionLanguagesCache = storedCache
    } else if (response.ok) {
      // 200 OK: Fresh response
      result = (await response.json()) as LanguagesResponse

      // Extract ETag for next request
      const etag = response.headers.get('ETag')
      const cacheData: CachedLanguages = {
        data: result,
        etag: etag ?? undefined,
        timestamp: Date.now(),
      }

      // Store in session cache with the freshly fetched ETag
      sessionLanguagesCache = cacheData

      // Store in localStorage for cross-session persistence
      if (typeof window !== 'undefined') {
        try {
          window.localStorage.setItem('lexicala-languages-cache-v1', JSON.stringify(cacheData))
        } catch (e) {
          // localStorage may be full or disabled, just skip persistence
          console.warn('Failed to cache languages to localStorage:', e)
        }
      }
    } else {
      // Unexpected status code; if we have cached data, use it
      if (storedCache) {
        result = storedCache.data
        sessionLanguagesCache = storedCache
      } else {
        throw new Error(`HTTP ${response.status}`)
      }
    }

    return result
  } catch (error) {
    // API failed; try to use cached version as fallback
    if (storedCache) {
      sessionLanguagesCache = storedCache
      return storedCache.data
    }

    // No fallback available, throw error
    throw error
  }
}

const api = {
  async createRound(language: string): Promise<ApiResult<CreateRoundResponse>> {
    const response = await fetch('/game/sense-sprint/rounds', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ language }),
    })

    return parseResponse<CreateRoundResponse>(response)
  },

  async languages(): Promise<ApiResult<LanguagesResponse>> {
    const data = await loadLanguagesWithCaching()
    return {
      data,
      rateLimit: null, // Cached requests don't consume rate limit quota
    }
  },

  async nextClue(roundId: string): Promise<ApiResult<NextClueResponse>> {
    const response = await fetch(`/game/sense-sprint/rounds/${roundId}/clues/next`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    return parseResponse<NextClueResponse>(response)
  },

  async guess(roundId: string, guessText: string): Promise<ApiResult<GuessResponse>> {
    const response = await fetch(`/game/sense-sprint/rounds/${roundId}/guess`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ guess: guessText }),
    })

    return parseResponse<GuessResponse>(response)
  },

  async giveUp(roundId: string): Promise<ApiResult<GuessResponse>> {
    const response = await fetch(`/game/sense-sprint/rounds/${roundId}/give-up`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    return parseResponse<GuessResponse>(response)
  },

  async createQuizRound(targetLanguage?: string): Promise<ApiResult<CreateQuizRoundResponse>> {
    const url = targetLanguage
      ? `/game/translation-quiz/rounds?targetLanguage=${encodeURIComponent(targetLanguage)}`
      : '/game/translation-quiz/rounds'
    const response = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    return parseResponse<CreateQuizRoundResponse>(response)
  },

  async submitQuizAnswer(roundId: string, choice: string): Promise<ApiResult<QuizAnswerResponse>> {
    const response = await fetch(`/game/translation-quiz/rounds/${roundId}/answer`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ choice }),
    })

    return parseResponse<QuizAnswerResponse>(response)
  },

  async expireQuizRound(roundId: string): Promise<ApiResult<QuizAnswerResponse>> {
    const response = await fetch(`/game/translation-quiz/rounds/${roundId}/expire`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    return parseResponse<QuizAnswerResponse>(response)
  },
}

function getRateLimitFromHeaders(headers: Headers): RateLimitDebug | null {
  const limitRaw = headers.get('X-RateLimit-requests-Limit')
  const remainingRaw = headers.get('X-RateLimit-requests-Remaining')
  const resetRaw = headers.get('X-RateLimit-requests-Reset')

  if (!limitRaw || !remainingRaw || !resetRaw) {
    return null
  }

  const limit = Number.parseInt(limitRaw, 10)
  const limitRemaining = Number.parseInt(remainingRaw, 10)
  const resetSeconds = Number.parseInt(resetRaw, 10)

  if (!Number.isFinite(limit) || !Number.isFinite(limitRemaining) || !Number.isFinite(resetSeconds)) {
    return null
  }

  return { limit, limitRemaining, resetSeconds }
}

async function parseResponse<T>(response: Response): Promise<ApiResult<T>> {
  if (!response.ok) {
    const errorPayload = (await response.json().catch(() => null)) as ProblemResponse | null
    const detail = errorPayload?.detail ?? errorPayload?.title ?? 'Request failed'
    throw new Error(detail)
  }

  const data = (await response.json()) as T
  return {
    data,
    rateLimit: getRateLimitFromHeaders(response.headers),
  }
}

// ─── SenseSprint component ────────────────────────────────────────────────────

function SenseSprint() {
  const [round, setRound] = useState<ActiveRound | null>(null)
  const [guess, setGuess] = useState('')
  const [selectedLanguage, setSelectedLanguage] = useState('en')
  const [languageOptions, setLanguageOptions] = useState<LanguageOption[]>([{ code: 'en', name: 'English' }])
  const [loadingLanguages, setLoadingLanguages] = useState(true)
  const [statusMessage, setStatusMessage] = useState('Start a round to begin.')
  const [feedbackTone, setFeedbackTone] = useState<FeedbackTone>('info')
  const [stats, setStats] = useState<GameStats>(() => loadStats())
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [timeLeft, setTimeLeft] = useState(0)
  const [rateLimitDebug, setRateLimitDebug] = useState<RateLimitDebug | null>(null)
  const [revealedClues, setRevealedClues] = useState<string[]>([])
  const [wrongGuesses, setWrongGuesses] = useState<string[]>([])
  const [debugCallCount, setDebugCallCount] = useState(0)
  const [debugLastUpdated, setDebugLastUpdated] = useState('-')
  const [debugPulseTick, setDebugPulseTick] = useState(0)
  const [hasHydratedSession, setHasHydratedSession] = useState(false)
  const expiryHandledRef = useRef(false)
  const [statsOpen, setStatsOpen] = useState(() => {
    if (typeof window === 'undefined') return true
    return window.innerWidth >= 760
  })

  useEffect(() => {
    if (typeof window === 'undefined') {
      return
    }

    window.localStorage.setItem(statsStorageKey, JSON.stringify(stats))
  }, [stats])

  useEffect(() => {
    let isCancelled = false

    const loadLanguages = async () => {
      setLoadingLanguages(true)
      try {
        // Calls loadLanguagesWithCaching():
        // 1️⃣ Session cache (no API call if loaded in this session)
        // 2️⃣ localStorage with ETag (304 Not Modified if unchanged)
        // 3️⃣ API fallback, saves ETag for next time
        const languageResult = await api.languages()
        if (isCancelled) {
          return
        }

        console.log('Languages response:', languageResult.data)
        const languageNames = languageResult.data.language_names ?? {}
        const sourceLanguages = languageResult.data.resources?.global?.source_languages ?? []
        console.log('Parsed sourceLanguages:', sourceLanguages)
        const options = sourceLanguages
          .map((code) => code.trim().toLowerCase())
          .filter((code, index, current) => code.length > 0 && current.indexOf(code) === index)
          .map((code) => ({
            code,
            name: languageNames[code] ?? code.toUpperCase(),
          }))
          .sort((left, right) => left.name.localeCompare(right.name))

        const safeOptions = options.length > 0 ? options : [{ code: 'en', name: 'English' }]
        setLanguageOptions(safeOptions)
        setSelectedLanguage((current) => {
          if (safeOptions.some((option) => option.code === current)) {
            return current
          }

          const englishFallback = safeOptions.find((option) => option.code === 'en')
          return englishFallback?.code ?? safeOptions[0].code
        })
      } catch (err) {
        console.error('Failed to load languages:', err)
        if (!isCancelled) {
          setLanguageOptions([{ code: 'en', name: 'English' }])
          setSelectedLanguage('en')
        }
      } finally {
        if (!isCancelled) {
          setLoadingLanguages(false)
        }
      }
    }

    void loadLanguages()

    return () => {
      isCancelled = true
    }
  }, [])

  useEffect(() => {
    if (hasHydratedSession) {
      return
    }

    setHasHydratedSession(true)
    const storedSession = loadStoredSession()
    if (!storedSession) {
      return
    }

    if (getSecondsRemaining(storedSession.round.expiresAtUtc) <= 0) {
      clearStoredSession()
      setStats((current) => ({
        ...current,
        roundsExpired: current.roundsExpired + 1,
        currentStreak: 0,
        lastPlayedAt: new Date().toISOString(),
      }))
      setStatusMessage(
        stats.currentStreak > 0
          ? `Saved round expired while you were away. ${stats.currentStreak}-win streak reset.`
          : 'Saved round expired while you were away. Start a new round.',
      )
      setFeedbackTone('warning')
      return
    }

    setRound(storedSession.round)
    setSelectedLanguage(storedSession.round.language)
    setRevealedClues(storedSession.revealedClues.length > 0 ? storedSession.revealedClues : [storedSession.round.clue])
    setWrongGuesses(storedSession.wrongGuesses)
    setGuess(storedSession.guess)
    setTimeLeft(getSecondsRemaining(storedSession.round.expiresAtUtc))
    setStatusMessage('Round restored. Pick up where you left off.')
    setFeedbackTone('info')
  }, [hasHydratedSession, stats.currentStreak])

  useEffect(() => {
    if (typeof window === 'undefined') {
      return
    }

    if (!round || round.roundStatus !== 'in-progress') {
      clearStoredSession()
      return
    }

    const sessionPayload: StoredSession = {
      round,
      revealedClues: revealedClues.length > 0 ? revealedClues : [round.clue],
      wrongGuesses,
      guess,
    }

    window.localStorage.setItem(sessionStorageKey, JSON.stringify(sessionPayload))
  }, [round, revealedClues, wrongGuesses, guess])

  useEffect(() => {
    expiryHandledRef.current = false
  }, [round?.roundId, round?.roundStatus])

  useEffect(() => {
    if (!round || round.roundStatus !== 'in-progress') {
      return
    }

    const tick = () => {
      const diff = getSecondsRemaining(round.expiresAtUtc)
      setTimeLeft(diff)

      if (diff > 0) {
        return
      }

      if (expiryHandledRef.current) {
        return
      }

      expiryHandledRef.current = true

      // Auto-giveup when timer expires to reveal answer
      setLoading(true)
      api
        .giveUp(round.roundId)
        .then((result) => {
          const streakBeforeExpiry = stats.currentStreak

          setRound((current) => {
            if (!current || current.roundId !== round.roundId) {
              return current
            }

            return {
              ...current,
              roundStatus: 'expired',
              answer: result.data.correctAnswer,
            }
          })

          setStats((current) => ({
            ...current,
            roundsExpired: current.roundsExpired + 1,
            currentStreak: 0,
            lastPlayedAt: new Date().toISOString(),
          }))

          setStatusMessage(
            streakBeforeExpiry > 0
              ? `Time expired. ${streakBeforeExpiry}-win streak reset.`
              : 'Time expired. Start a new round.',
          )
          setFeedbackTone('warning')
          setGuess('')
          clearStoredSession()
        })
        .catch((err) => {
          const message = err instanceof Error ? err.message : String(err)
          const isRoundAlreadyGone = message.toLowerCase().includes('round not found')

          const streakBeforeExpiry = stats.currentStreak

          setRound((current) => {
            if (!current || current.roundId !== round.roundId) {
              return current
            }

            return {
              ...current,
              roundStatus: 'expired',
            }
          })

          setStats((current) => ({
            ...current,
            roundsExpired: current.roundsExpired + 1,
            currentStreak: 0,
            lastPlayedAt: new Date().toISOString(),
          }))

          setStatusMessage(
            streakBeforeExpiry > 0
              ? `Time expired. ${streakBeforeExpiry}-win streak reset.`
              : 'Time expired. Start a new round.',
          )
          setFeedbackTone('warning')
          setGuess('')
          setError('')
          clearStoredSession()

          if (!isRoundAlreadyGone) {
            console.warn('Timer expiry fallback: round marked expired without give-up payload.', err)
          }
        })
        .finally(() => {
          setLoading(false)
        })
    }

    tick()
    const timer = window.setInterval(() => {
      tick()
    }, 250)

    return () => window.clearInterval(timer)
  }, [round, stats.currentStreak])

  const canInteract = useMemo(
    () => Boolean(round && round.roundStatus === 'in-progress' && !loading),
    [round, loading],
  )

  const streakMultiplier = useMemo(() => getStreakMultiplier(stats.currentStreak), [stats.currentStreak])
  const scoreIfCorrect = useMemo(
    () => (round ? getAdjustedScore(round.scoreIfCorrect, stats.currentStreak) : 0),
    [round, stats.currentStreak],
  )
  const clueStep = round ? `${round.clueIndex + 1}/${round.maxClues}` : '0/0'
  const usedRequests = rateLimitDebug ? Math.max(0, rateLimitDebug.limit - rateLimitDebug.limitRemaining) : null
  const celebrationLevel = round?.roundStatus === 'won' ? getCelebrationLevel(stats.currentStreak) : 'none'
  const averageCluesPerRound = stats.roundsStarted > 0 ? stats.totalCluesSeen / stats.roundsStarted : 0
  const averageRequestedCluesPerRound = stats.roundsStarted > 0 ? stats.cluesRequested / stats.roundsStarted : 0
  const nextCelebrationAt = Math.floor(stats.currentStreak / 5) * 5 + 5
  const winsUntilCelebration = nextCelebrationAt - stats.currentStreak
  const safeSelectedLanguage = typeof selectedLanguage === 'string' && selectedLanguage.trim().length > 0 ? selectedLanguage : 'en'
  const selectedLanguageName = languageOptions.find((option) => option.code === safeSelectedLanguage)?.name ?? safeSelectedLanguage.toUpperCase()

  function refreshDebug(rateLimit: RateLimitDebug | null): void {
    setDebugCallCount((current) => current + 1)
    setDebugLastUpdated(new Date().toLocaleTimeString())
    setDebugPulseTick((current) => current + 1)
    if (rateLimit) {
      setRateLimitDebug(rateLimit)
    }
  }

  async function startRound(): Promise<void> {
    const isAbandoningActiveRound = round?.roundStatus === 'in-progress'
    const streakBeforeReset = stats.currentStreak
    const requestedLanguage = safeSelectedLanguage

    if (isAbandoningActiveRound) {
      const confirmed = window.confirm(
        streakBeforeReset > 0
          ? `Starting a new word will reset your ${streakBeforeReset}-win streak. Continue?`
          : 'Starting a new word will abandon the current round. Continue?',
      )

      if (!confirmed) {
        return
      }
    }

    setLoading(true)
    setError('')

    try {
      const createdResult = await api.createRound(requestedLanguage)
      const created = createdResult.data
      const resolvedLanguage = typeof created.language === 'string' && created.language.trim().length > 0 ? created.language : requestedLanguage
      setRound({
        roundId: created.roundId,
        language: resolvedLanguage,
        clue: created.clue,
        clueIndex: created.clueIndex,
        maxClues: created.maxClues,
        expiresAtUtc: created.expiresAtUtc,
        scoreIfCorrect: created.scoreIfCorrect,
        roundStatus: 'in-progress',
        answer: null,
      })
      setSelectedLanguage(resolvedLanguage)
      setRevealedClues([created.clue])
      setWrongGuesses([])
      setTimeLeft(getSecondsRemaining(created.expiresAtUtc))
      refreshDebug(createdResult.rateLimit ?? created.rateLimit)

      setStats((current) => ({
        ...current,
        roundsStarted: current.roundsStarted + 1,
        roundsAbandoned: current.roundsAbandoned + (isAbandoningActiveRound ? 1 : 0),
        currentStreak: isAbandoningActiveRound ? 0 : current.currentStreak,
        totalCluesSeen: current.totalCluesSeen + 1,
        lastPlayedAt: new Date().toISOString(),
      }))
      setGuess('')
      if (isAbandoningActiveRound && streakBeforeReset > 0) {
        setStatusMessage(`New round started. ${streakBeforeReset}-win streak reset.`)
        setFeedbackTone('warning')
      } else if (isAbandoningActiveRound) {
        setStatusMessage('New round started. Previous word skipped.')
        setFeedbackTone('warning')
      } else {
        setStatusMessage('Round started. Read the clue and submit your best guess.')
        setFeedbackTone('info')
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to start round.'
      setError(message)
      setFeedbackTone('error')
    } finally {
      setLoading(false)
    }
  }

  async function revealNextClue(): Promise<void> {
    if (!round) {
      return
    }

    setLoading(true)
    setError('')

    try {
      const nextResult = await api.nextClue(round.roundId)
      const next = nextResult.data
      setRound((current) => {
        if (!current) {
          return current
        }

        return {
          ...current,
          clue: next.clue,
          clueIndex: next.clueIndex,
          scoreIfCorrect: next.scoreIfCorrect,
          expiresAtUtc: next.expiresAtUtc,
        }
      })
      setRevealedClues((current) => {
        if (current.includes(next.clue)) {
          return current
        }

        return [...current, next.clue]
      })
      refreshDebug(nextResult.rateLimit)
      setTimeLeft(getSecondsRemaining(next.expiresAtUtc))
      setStats((current) => ({
        ...current,
        cluesRequested: current.cluesRequested + 1,
        totalCluesSeen: current.totalCluesSeen + 1,
        lastPlayedAt: new Date().toISOString(),
      }))
      setStatusMessage('New clue revealed.')
      setFeedbackTone('info')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to reveal next clue.'
      setError(message)
      setFeedbackTone('error')
    } finally {
      setLoading(false)
    }
  }

  async function submitGuess(): Promise<void> {
    if (!round || !guess.trim()) {
      return
    }

    const streakBeforeGuess = stats.currentStreak
    const submittedGuess = guess.trim()

    setLoading(true)
    setError('')

    try {
      const guessResult = await api.guess(round.roundId, submittedGuess)
      const result = guessResult.data

      setStats((current) => ({
        ...current,
        totalGuessesSubmitted: current.totalGuessesSubmitted + 1,
        lastPlayedAt: new Date().toISOString(),
      }))

      if (result.isCorrect) {
        const awardedPoints = getAdjustedScore(result.awardedPoints, streakBeforeGuess)
        const nextStreak = streakBeforeGuess + 1

        setStats((current) => ({
          ...current,
          currentScore: current.currentScore + awardedPoints,
          highScore: Math.max(current.highScore, awardedPoints),
          roundsWon: current.roundsWon + 1,
          wordsGuessedCorrectly: current.wordsGuessedCorrectly + 1,
          currentStreak: nextStreak,
          highestStreak: Math.max(current.highestStreak, nextStreak),
          majorCelebrations: current.majorCelebrations + (nextStreak % 5 === 0 ? 1 : 0),
          lastPlayedAt: new Date().toISOString(),
        }))

        setStatusMessage(
          nextStreak % 5 === 0
            ? `Correct! +${awardedPoints} points. ${nextStreak}-win streak. Major celebration unlocked.`
            : `Correct! +${awardedPoints} points. Streak is now ${nextStreak}.`,
        )
        setFeedbackTone('success')
      } else {
        setWrongGuesses((current) => [...current, submittedGuess])
      }

      setRound((current) => {
        if (!current) {
          return current
        }

        return {
          ...current,
          roundStatus: result.roundStatus,
          answer: result.correctAnswer,
        }
      })
      refreshDebug(guessResult.rateLimit)

      if (result.roundStatus === 'lost') {
        setStats((current) => ({
          ...current,
          roundsLost: current.roundsLost + 1,
          currentStreak: 0,
          lastPlayedAt: new Date().toISOString(),
        }))
        setStatusMessage(
          streakBeforeGuess > 0
            ? `No more clues. Answer: ${result.correctAnswer ?? 'unknown'}. ${streakBeforeGuess}-win streak reset.`
            : `No more clues. Answer: ${result.correctAnswer ?? 'unknown'}`,
        )
        setFeedbackTone('warning')
      } else if (result.roundStatus === 'expired') {
        setStats((current) => ({
          ...current,
          roundsExpired: current.roundsExpired + 1,
          currentStreak: 0,
          lastPlayedAt: new Date().toISOString(),
        }))
        setStatusMessage(
          streakBeforeGuess > 0 ? `Round expired. ${streakBeforeGuess}-win streak reset.` : 'Round expired. Start a new round.',
        )
        setFeedbackTone('warning')
      } else if (result.roundStatus !== 'won') {
        setStatusMessage('Not correct yet. Ask for the next clue or try again.')
        setFeedbackTone('info')
      }

      setGuess('')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to submit guess.'
      setError(message)
      setFeedbackTone('error')
    } finally {
      setLoading(false)
    }
  }

  async function giveUpRound(): Promise<void> {
    if (!round) {
      return
    }

    const streakBeforeGiveUp = stats.currentStreak

    setLoading(true)
    setError('')

    try {
      const giveUpResult = await api.giveUp(round.roundId)
      const result = giveUpResult.data
      setRound((current) => {
        if (!current) {
          return current
        }

        return {
          ...current,
          roundStatus: result.roundStatus,
          answer: result.correctAnswer,
        }
      })
      refreshDebug(giveUpResult.rateLimit)

      setStats((current) => ({
        ...current,
        roundsGivenUp: current.roundsGivenUp + 1,
        currentStreak: 0,
        lastPlayedAt: new Date().toISOString(),
      }))

      setStatusMessage(
        streakBeforeGiveUp > 0
          ? `You gave up. Answer: ${result.correctAnswer ?? 'unknown'}. ${streakBeforeGiveUp}-win streak reset.`
          : `You gave up. Answer: ${result.correctAnswer ?? 'unknown'}`,
      )
      setFeedbackTone('warning')
      setGuess('')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to give up this round.'
      setError(message)
      setFeedbackTone('error')
    } finally {
      setLoading(false)
    }
  }

  function resetStats(): void {
    const hasActiveRound = round?.roundStatus === 'in-progress'
    const confirmed = window.confirm(
      hasActiveRound
        ? 'Reset all saved stats? This clears score, streak, and high score, but keeps the current word active.'
        : 'Reset all saved stats, including score, streak, and high score?',
    )

    if (!confirmed) {
      return
    }

    if (typeof window !== 'undefined') {
      window.localStorage.removeItem(statsStorageKey)
    }

    setStats({
      ...defaultStats,
      lastPlayedAt: new Date().toISOString(),
    })
    setStatusMessage(hasActiveRound ? 'Lifetime stats reset. Current round preserved.' : 'Lifetime stats reset.')
    setFeedbackTone('warning')
    setError('')
  }

  return (
    <section className="panel grid">
      <details open={statsOpen} onToggle={(e) => setStatsOpen(e.currentTarget.open)} className="stats-collapsible">
        <summary className="stats-summary">Performance Snapshot</summary>
        <div className="stats">
        <article className="kpi">
          <p className="kpi-label">Total Points</p>
          <p className="kpi-value">{stats.currentScore}</p>
        </article>
        <article className="kpi">
            <p className="kpi-label">High Score</p>
            <p className="kpi-value">{stats.highScore}</p>
          </article>
          <article className="kpi">
            <p className="kpi-label">Win Streak</p>
            <p className="kpi-value">{stats.currentStreak}</p>
          </article>
          <article className="kpi">
            <p className="kpi-label">Highest Streak</p>
            <p className="kpi-value">{stats.highestStreak}</p>
          </article>
          <article className="kpi">
            <p className="kpi-label">Games Played</p>
            <p className="kpi-value">{stats.roundsStarted}</p>
          </article>
          <article className="kpi">
            <p className="kpi-label">Time Left</p>
            <p className="kpi-value">{round ? `${timeLeft}s` : '-'}</p>
          </article>
        </div>

        <section className={`streak-panel streak-panel-${celebrationLevel}`} aria-live="polite">
          <div>
            <p className="streak-label">Streak Bonus</p>
            <p className="streak-value">{streakMultiplier.toFixed(2)}x scoring</p>
            <p className="streak-copy">
              Bonus plan: 3 wins unlock 1.15x, 5 wins unlock 1.35x and a major celebration, 10 wins reach 1.65x, and 15 wins push to 2.00x.
            </p>
            <p className="streak-copy">The current round is also saved locally, so a reload resumes the active word and revealed clues.</p>
          </div>
          <div className="streak-meta">
            <p>
              <strong>Next celebration:</strong> {winsUntilCelebration} win{winsUntilCelebration === 1 ? '' : 's'} away
            </p>
            <p>
              <strong>Major celebrations:</strong> {stats.majorCelebrations}
            </p>
            <p>
              <strong>Last played:</strong> {stats.lastPlayedAt ? new Date(stats.lastPlayedAt).toLocaleString() : 'Not yet'}
            </p>
          </div>
        </section>
        <div className="meta-actions">
          <button className="button button-reset" onClick={resetStats} disabled={loading}>
            Reset Stats
          </button>
        </div>
        </details>

        <div className="clue">
          <p className="clue-label">Clue {clueStep}</p>
          <p className="clue-text">{round?.clue ?? 'No active round yet.'}</p>
          <div className="clue-history" aria-live="polite">
            <p className="clue-history-label">Revealed clues</p>
            {revealedClues.length > 0 ? (
              <ol className="clue-history-list">
                {revealedClues.map((revealedClue, index) => (
                  <li key={`${index}-${revealedClue}`}>
                    <span className="clue-history-index">#{index + 1}</span>
                    <span>{revealedClue}</span>
                  </li>
                ))}
              </ol>
            ) : (
              <p className="clue-history-empty">No clues revealed yet.</p>
            )}

            <p className="clue-history-label wrong-history-label">Wrong answers entered</p>
            {wrongGuesses.length > 0 ? (
              <ol className="clue-history-list wrong-history-list">
                {wrongGuesses.map((wrongGuess, index) => (
                  <li key={`${index}-${wrongGuess}`}>
                    <span className="clue-history-index wrong-history-index">#{index + 1}</span>
                    <span>{wrongGuess}</span>
                  </li>
                ))}
              </ol>
            ) : (
              <p className="clue-history-empty">No wrong answers yet.</p>
            )}
          </div>
        </div>

        <div className="controls">
          <label className="language-picker" htmlFor="language-select">
            <span>Round Language</span>
            <select
              id="language-select"
              className="input"
              value={selectedLanguage}
              onChange={(event) => setSelectedLanguage(event.target.value)}
              disabled={loading || loadingLanguages || round?.roundStatus === 'in-progress'}
            >
              {languageOptions.map((option) => (
                <option key={option.code} value={option.code}>
                  {option.name} ({option.code})
                </option>
              ))}
            </select>
          </label>

          <input
            className="input"
            placeholder="Type your guess"
            value={guess}
            onChange={(event) => setGuess(event.target.value)}
            disabled={!canInteract}
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                void submitGuess()
              }
            }}
          />

          <div className="actions">
            <button className="button button-primary" onClick={() => void submitGuess()} disabled={!canInteract || !guess.trim()}>
              Submit Guess
            </button>
            <button
              className="button button-secondary"
              onClick={() => void revealNextClue()}
              disabled={!canInteract || (round?.clueIndex ?? 0) >= (round?.maxClues ?? 1) - 1}
            >
              Next Clue
            </button>
            <button className="button button-warning" onClick={() => void giveUpRound()} disabled={!canInteract}>
              Give Up
            </button>
            <button className="button button-ghost" onClick={() => void startRound()} disabled={loading}>
              {round ? 'New Round' : 'Start Round'}
            </button>
          </div>
        </div>

        <p
          className={`status status-${feedbackTone} ${round?.roundStatus === 'won' ? 'status-win' : ''} ${celebrationLevel === 'major' ? 'status-major-win' : ''}`}
          role="status"
          aria-live="polite"
        >
          <strong>Status:</strong> {statusMessage}
        </p>
        {round?.roundStatus === 'won' ? (
          <p className={`victory-banner ${celebrationLevel === 'major' ? 'victory-banner-major' : ''}`}>
            {celebrationLevel === 'major'
              ? `Word cracked. ${stats.currentStreak}-win streak. Fire the confetti cannons.`
              : 'Word cracked. Keep the streak going!'}
          </p>
        ) : null}
        {error ? <p className="error">{error}</p> : null}
        <details className="metrics-collapsible">
          <summary className="metrics-summary">Lifetime Statistics</summary>
          <section className="metrics-grid" aria-label="Lifetime statistics">
          <article className="metric-card">
            <p className="metric-label">Words Guessed</p>
            <p className="metric-value">{stats.wordsGuessedCorrectly}</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Wins / Losses</p>
            <p className="metric-value">{stats.roundsWon} / {stats.roundsLost}</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Expired / Give Ups</p>
            <p className="metric-value">{stats.roundsExpired} / {stats.roundsGivenUp}</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Abandoned Rounds</p>
            <p className="metric-value">{stats.roundsAbandoned}</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Clues Requested</p>
            <p className="metric-value">{stats.cluesRequested}</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Guesses Submitted</p>
            <p className="metric-value">{stats.totalGuessesSubmitted}</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Avg. Clues / Round</p>
            <p className="metric-value">{formatAverage(averageCluesPerRound)}</p>
          </article>
          <article className="metric-card">
            <p className="metric-label">Avg. Extra Clues / Round</p>
            <p className="metric-value">{formatAverage(averageRequestedCluesPerRound)}</p>
          </article>
        </section>
        </details>
        <p className="footer">
          Active language: {round?.language?.toUpperCase() ?? selectedLanguageName}.
          {' '}
          Points available now: {scoreIfCorrect}
          {round ? ` (${round.scoreIfCorrect} base x ${streakMultiplier.toFixed(2)} streak bonus)` : ''}
        </p>

        <details className={`debug-panel ${debugPulseTick % 2 === 0 ? 'debug-panel-pulse-a' : 'debug-panel-pulse-b'}`}>
          <summary>Debug Info</summary>
          <div className="debug-grid">
            <p>
              <strong>Round ID:</strong> {round?.roundId ?? '-'}
            </p>
            <p>
              <strong>Round Status:</strong> {round?.roundStatus ?? '-'}
            </p>
            <p>
              <strong>Rate Limit:</strong> {rateLimitDebug ? `${rateLimitDebug.limitRemaining}/${rateLimitDebug.limit} remaining` : 'not available yet'}
            </p>
            <p>
              <strong>Rate Used:</strong> {usedRequests ?? '-'}
            </p>
            <p>
              <strong>Rate Reset (sec):</strong> {rateLimitDebug?.resetSeconds ?? '-'}
            </p>
            <p>
              <strong>API Calls:</strong> {debugCallCount}
            </p>
            <p>
              <strong>Debug Updated:</strong> {debugLastUpdated}
            </p>
          </div>
        </details>
    </section>
  )
}

// ─── Translation Quiz component ───────────────────────────────────────────────

function TranslationQuiz() {
  const [round, setRound] = useState<ActiveQuizRound | null>(null)
  const [targetLanguage, setTargetLanguage] = useState('')
  const [quizLanguageOptions, setQuizLanguageOptions] = useState<LanguageOption[]>([])
  const [loadingQuizLanguages, setLoadingQuizLanguages] = useState(true)
  const [statusMessage, setStatusMessage] = useState('Choose a target language (or leave on Random) and start a round.')
  const [feedbackTone, setFeedbackTone] = useState<FeedbackTone>('info')
  const [stats, setStats] = useState<QuizStats>(() => loadQuizStats())
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [timeLeft, setTimeLeft] = useState(0)
  const [rateLimitDebug, setRateLimitDebug] = useState<RateLimitDebug | null>(null)
  const [debugCallCount, setDebugCallCount] = useState(0)
  const [debugLastUpdated, setDebugLastUpdated] = useState('-')
  const [debugPulseTick, setDebugPulseTick] = useState(0)
  const expiryHandledRef = useRef(false)

  useEffect(() => {
    saveQuizStats(stats)
  }, [stats])

  // Load available target languages from the API
  useEffect(() => {
    let isCancelled = false

    const loadLanguages = async () => {
      setLoadingQuizLanguages(true)
      try {
        // Calls loadLanguagesWithCaching():
        // 1️⃣ Session cache (no API call if loaded in this session)
        // 2️⃣ localStorage with ETag (304 Not Modified if unchanged)
        // 3️⃣ API fallback, saves ETag for next time
        const result = await api.languages()
        if (isCancelled) return

        const languageNames = result.data.language_names ?? {}
        // Prefer explicit target_languages; fall back to source_languages, exclude 'en'
        const rawList = [
          ...new Set(
            (result.data.resources?.global?.target_languages ?? result.data.resources?.global?.source_languages ?? [])
              .map((code) => code.trim().toLowerCase())
              .filter((code) => code.length > 0 && code !== 'en'),
          ),
        ].sort()
        const options = rawList.map((code) => ({ code, name: languageNames[code] ?? code.toUpperCase() }))
        setQuizLanguageOptions(options)
      } catch {
        if (!isCancelled) {
          setQuizLanguageOptions([
            { code: 'de', name: 'German' },
            { code: 'nl', name: 'Dutch' },
            { code: 'fr', name: 'French' },
            { code: 'es', name: 'Spanish' },
          ])
        }
      } finally {
        if (!isCancelled) setLoadingQuizLanguages(false)
      }
    }

    void loadLanguages()
    return () => { isCancelled = true }
  }, [])

  useEffect(() => {
    if (!round || round.roundStatus !== 'in-progress') {
      return
    }

    const timer = window.setInterval(() => {
      const diff = Math.max(0, Math.ceil((new Date(round.expiresAtUtc).getTime() - Date.now()) / 1000))
      setTimeLeft(diff)
    }, 250)

    return () => window.clearInterval(timer)
  }, [round])

  // Reveal answer when timer expires
  useEffect(() => {
    if (timeLeft !== 0 || round?.roundStatus !== 'in-progress' || expiryHandledRef.current) {
      return
    }

    expiryHandledRef.current = true
    const roundId = round.roundId

    setLoading(true)
    api
      .expireQuizRound(roundId)
      .then((result) => {
        const data = result.data
        setRound((current) => {
          if (!current) return current
          return { ...current, roundStatus: data.roundStatus, correctAnswer: data.correctAnswer }
        })
        if (result.rateLimit) setRateLimitDebug(result.rateLimit)
        setDebugCallCount((n) => n + 1)
        setDebugLastUpdated(new Date().toLocaleTimeString())
        setDebugPulseTick((n) => n + 1)
        setStatusMessage(`Time's up! The correct answer was: ${data.correctAnswer}`)
        setFeedbackTone('warning')
        setStats((current) => ({ ...current, currentStreak: 0 }))
      })
      .catch(() => {
        setRound((current) => {
          if (!current) return current
          return { ...current, roundStatus: 'expired' }
        })
        setStatusMessage("Time's up!")
        setFeedbackTone('warning')
        setStats((current) => ({ ...current, currentStreak: 0 }))
      })
      .finally(() => {
        setLoading(false)
      })
    // expiryHandledRef guards against re-entry; loading is intentionally omitted
    // from deps to prevent the effect cleanup from cancelling an in-flight API call.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeLeft, round?.roundStatus, round?.roundId])

  function getLanguageName(code: string): string {
    return quizLanguageOptions.find((o) => o.code === code)?.name ?? code.toUpperCase()
  }

  function refreshDebug(rateLimit: RateLimitDebug | null): void {
    setDebugCallCount((current) => current + 1)
    setDebugLastUpdated(new Date().toLocaleTimeString())
    setDebugPulseTick((current) => current + 1)
    if (rateLimit) {
      setRateLimitDebug(rateLimit)
    }
  }

  async function startRound(): Promise<void> {
    setLoading(true)
    setError('')
    expiryHandledRef.current = false

    try {
      // Empty string → server picks a random language
      const result = await api.createQuizRound(targetLanguage || undefined)
      const created = result.data
      setRound({
        roundId: created.roundId,
        sourceWord: created.sourceWord,
        sourceLanguage: created.sourceLanguage,
        targetLanguage: created.targetLanguage,
        choices: created.choices,
        expiresAtUtc: created.expiresAtUtc,
        roundStatus: 'in-progress',
        correctAnswer: null,
        selectedChoice: null,
      })
      setTimeLeft(Math.max(0, Math.ceil((new Date(created.expiresAtUtc).getTime() - Date.now()) / 1000)))
      refreshDebug(result.rateLimit ?? created.rateLimit)
      setStats((current) => ({ ...current, roundsPlayed: current.roundsPlayed + 1 }))
      setStatusMessage(`Choose the correct ${getLanguageName(created.targetLanguage)} translation.`)
      setFeedbackTone('info')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to start round.'
      setError(message)
      setFeedbackTone('error')
    } finally {
      setLoading(false)
    }
  }

  async function submitChoice(choice: string): Promise<void> {
    if (!round || round.roundStatus !== 'in-progress' || loading) {
      return
    }

    setLoading(true)
    setError('')

    try {
      const result = await api.submitQuizAnswer(round.roundId, choice)
      const data = result.data

      if (data.isCorrect) {
        const nextStreak = stats.currentStreak + 1
        setStats((current) => ({
          ...current,
          currentScore: current.currentScore + data.awardedPoints,
          highScore: Math.max(current.highScore, data.awardedPoints),
          roundsWon: current.roundsWon + 1,
          currentStreak: nextStreak,
          highestStreak: Math.max(current.highestStreak, nextStreak),
        }))
        setStatusMessage(`Correct! +${data.awardedPoints} ${data.awardedPoints === 1 ? 'point' : 'points'}. The translation is: ${data.correctAnswer}`)
        setFeedbackTone('success')
      } else {
        setStats((current) => ({ ...current, currentStreak: 0 }))
        if (data.roundStatus === 'expired') {
          setStatusMessage(`Time's up! The correct answer was: ${data.correctAnswer}`)
        } else {
          setStatusMessage(`Incorrect. The correct answer was: ${data.correctAnswer}`)
        }
        setFeedbackTone('warning')
      }

      setRound((current) => {
        if (!current) return current
        return {
          ...current,
          roundStatus: data.roundStatus,
          correctAnswer: data.correctAnswer,
          selectedChoice: choice,
        }
      })
      refreshDebug(result.rateLimit)
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to submit answer.'
      setError(message)
      setFeedbackTone('error')
    } finally {
      setLoading(false)
    }
  }

  const canAnswer = Boolean(round && round.roundStatus === 'in-progress' && !loading)
  const usedRequests = rateLimitDebug ? Math.max(0, rateLimitDebug.limit - rateLimitDebug.limitRemaining) : null

  return (
    <section className="panel grid">
      <div className="quiz-header">
        <label className="quiz-lang-label" htmlFor="quiz-target-language">
          Translate to:
        </label>
        <select
          id="quiz-target-language"
          className="quiz-lang-select"
          value={targetLanguage}
          onChange={(e) => setTargetLanguage(e.target.value)}
          disabled={loading || loadingQuizLanguages}
        >
          <option value="">Random (any language)</option>
          {quizLanguageOptions.map((option) => (
            <option key={option.code} value={option.code}>
              {option.name} ({option.code})
            </option>
          ))}
        </select>
      </div>

      <div className="stats">
        <article className="kpi">
          <p className="kpi-label">Total Points</p>
          <p className="kpi-value">{stats.currentScore}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">High Score</p>
          <p className="kpi-value">{stats.highScore}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Win Streak 🔥</p>
          <p className="kpi-value">{stats.currentStreak}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Best Streak</p>
          <p className="kpi-value">{stats.highestStreak}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Rounds</p>
          <p className="kpi-value">{stats.roundsPlayed}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Time Left</p>
          <p className="kpi-value">{round ? `${timeLeft}s` : '-'}</p>
        </article>
      </div>

      <div className="quiz-word-panel">
        {round ? (
          <>
            <p className="quiz-word-label">What is the {getLanguageName(round.targetLanguage)} translation of:</p>
            <p className="quiz-word">{round.sourceWord}</p>
          </>
        ) : (
          <p className="quiz-word-empty">No active round yet.</p>
        )}
      </div>

      <div className="quiz-choices" aria-label="Translation choices">
        {round?.choices.map((choice) => {
          const isSelected = round.selectedChoice === choice
          const isCorrect = round.correctAnswer === choice
          const isFinished = round.roundStatus !== 'in-progress'
          let choiceClass = 'button quiz-choice'
          if (isFinished) {
            if (isCorrect) choiceClass += ' quiz-choice-correct'
            else if (isSelected) choiceClass += ' quiz-choice-wrong'
          }
          return (
            <button
              key={choice}
              className={choiceClass}
              onClick={() => void submitChoice(choice)}
              disabled={!canAnswer}
            >
              {choice}
            </button>
          )
        }) ?? <p className="quiz-word-empty">Start a round to see choices.</p>}
      </div>

      <div className="actions">
        <button className="button button-ghost" onClick={() => void startRound()} disabled={loading}>
          {round ? 'New Round' : 'Start Round'}
        </button>
      </div>

      <p className={`status status-${feedbackTone} ${round?.roundStatus === 'won' ? 'status-win' : ''}`} role="status" aria-live="polite">
        <strong>Status:</strong> {statusMessage}
      </p>
      {round?.roundStatus === 'won' ? <p className="victory-banner">Correct! Streak: {stats.currentStreak} 🔥</p> : null}
      {error ? <p className="error">{error}</p> : null}

      <details className={`debug-panel ${debugPulseTick % 2 === 0 ? 'debug-panel-pulse-a' : 'debug-panel-pulse-b'}`}>
        <summary>Debug Info</summary>
        <div className="debug-grid">
          <p>
            <strong>Round ID:</strong> {round?.roundId ?? '-'}
          </p>
          <p>
            <strong>Round Status:</strong> {round?.roundStatus ?? '-'}
          </p>
          <p>
            <strong>Rate Limit:</strong> {rateLimitDebug ? `${rateLimitDebug.limitRemaining}/${rateLimitDebug.limit} remaining` : 'not available yet'}
          </p>
          <p>
            <strong>Rate Used:</strong> {usedRequests ?? '-'}
          </p>
          <p>
            <strong>Rate Reset (sec):</strong> {rateLimitDebug?.resetSeconds ?? '-'}
          </p>
          <p>
            <strong>API Calls:</strong> {debugCallCount}
          </p>
          <p>
            <strong>Debug Updated:</strong> {debugLastUpdated}
          </p>
        </div>
      </details>
    </section>
  )
}

// ─── App ──────────────────────────────────────────────────────────────────────

function App() {
  const [gameMode, setGameMode] = useState<GameMode>('sense-sprint')

  return (
    <main className="page">
      <header className="header">
        <span className="badge">Lexicala Game</span>
        <h1 className="title">{gameMode === 'sense-sprint' ? 'Sense Sprint' : 'Translation Quiz'}</h1>
        <p className="subtitle">
          {gameMode === 'sense-sprint'
            ? 'Guess the hidden word from lexical clues generated with Fluky Search.'
            : 'Select the correct translation from four options — race against the clock!'}
        </p>
      </header>

      <div className="mode-tabs" role="tablist" aria-label="Game mode">
        <button
          role="tab"
          aria-selected={gameMode === 'sense-sprint'}
          className={`mode-tab ${gameMode === 'sense-sprint' ? 'mode-tab-active' : ''}`}
          onClick={() => setGameMode('sense-sprint')}
        >
          Sense Sprint
        </button>
        <button
          role="tab"
          aria-selected={gameMode === 'translation-quiz'}
          className={`mode-tab ${gameMode === 'translation-quiz' ? 'mode-tab-active' : ''}`}
          onClick={() => setGameMode('translation-quiz')}
        >
          Translation Quiz
        </button>
      </div>

      {gameMode === 'sense-sprint' ? <SenseSprint /> : <TranslationQuiz />}
    </main>
  )
}

export default App
