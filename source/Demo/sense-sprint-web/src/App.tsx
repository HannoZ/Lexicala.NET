import { useEffect, useMemo, useState } from 'react'
import './App.css'

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
  guess: string
}

const statsStorageKey = 'sense-sprint-stats-v1'
const sessionStorageKey = 'sense-sprint-session-v1'

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
  return Math.max(0, Math.floor((new Date(expiresAtUtc).getTime() - Date.now()) / 1000))
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
    const response = await fetch('/languages')
    return parseResponse<LanguagesResponse>(response)
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

function App() {
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
  const [debugCallCount, setDebugCallCount] = useState(0)
  const [debugLastUpdated, setDebugLastUpdated] = useState('-')
  const [debugPulseTick, setDebugPulseTick] = useState(0)
  const [hasHydratedSession, setHasHydratedSession] = useState(false)
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
      guess,
    }

    window.localStorage.setItem(sessionStorageKey, JSON.stringify(sessionPayload))
  }, [round, revealedClues, guess])

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
          console.error('Failed to process round expiry:', err)
          setError(String(err))
          setStatusMessage('Round ended but could not retrieve answer.')
          setFeedbackTone('error')
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
  const selectedLanguageName = languageOptions.find((option) => option.code === selectedLanguage)?.name ?? selectedLanguage.toUpperCase()

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
      const createdResult = await api.createRound(selectedLanguage)
      const created = createdResult.data
      setRound({
        roundId: created.roundId,
        language: created.language,
        clue: created.clue,
        clueIndex: created.clueIndex,
        maxClues: created.maxClues,
        expiresAtUtc: created.expiresAtUtc,
        scoreIfCorrect: created.scoreIfCorrect,
        roundStatus: 'in-progress',
        answer: null,
      })
      setSelectedLanguage(created.language)
      setRevealedClues([created.clue])
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

    setLoading(true)
    setError('')

    try {
      const guessResult = await api.guess(round.roundId, guess)
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
    <main className="page">
      <header className="header">
        <span className="badge">Lexicala Game</span>
        <h1 className="title">Sense Sprint</h1>
        <p className="subtitle">
          Guess the hidden English word from lexical clues generated with Fluky Search.
        </p>
      </header>

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
    </main>
  )
}

export default App
