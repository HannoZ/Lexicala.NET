import { useEffect, useMemo, useRef, useState } from 'react'
import './App.css'

type GameMode = 'sense-sprint' | 'translation-quiz'
type RoundStatus = 'in-progress' | 'won' | 'lost' | 'expired' | 'completed'
type FeedbackTone = 'info' | 'success' | 'warning' | 'error'

type RateLimitDebug = {
  limit: number
  limitRemaining: number
  resetSeconds: number
}

type ActiveRound = {
  roundId: string
  clue: string
  clueIndex: number
  maxClues: number
  expiresAtUtc: string
  scoreIfCorrect: number
  roundStatus: RoundStatus
  answer: string | null
}

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

type CreateRoundResponse = {
  roundId: string
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

type ProblemResponse = {
  title?: string
  detail?: string
}

type ApiResult<T> = {
  data: T
  rateLimit: RateLimitDebug | null
}

const SUPPORTED_LANGUAGES: Record<string, string> = {
  de: 'German',
  nl: 'Dutch',
  fr: 'French',
  es: 'Spanish',
}

const HIGH_SCORE_KEY = 'senseSprintHighScore'
const QUIZ_HIGH_SCORE_KEY = 'translationQuizHighScore'

const api = {
  async createRound(): Promise<ApiResult<CreateRoundResponse>> {
    const response = await fetch('/game/sense-sprint/rounds', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    return parseResponse<CreateRoundResponse>(response)
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

  async createQuizRound(targetLanguage: string): Promise<ApiResult<CreateQuizRoundResponse>> {
    const response = await fetch(`/game/translation-quiz/rounds?targetLanguage=${encodeURIComponent(targetLanguage)}`, {
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

function loadHighScore(key: string): number {
  try {
    return Number.parseInt(localStorage.getItem(key) ?? '0', 10) || 0
  } catch {
    return 0
  }
}

function saveHighScore(key: string, score: number): void {
  try {
    localStorage.setItem(key, String(score))
  } catch {
    // localStorage unavailable, ignore
  }
}

// ─── Sense Sprint Component ───────────────────────────────────────────────────

function SenseSprint() {
  const [round, setRound] = useState<ActiveRound | null>(null)
  const [guess, setGuess] = useState('')
  const [statusMessage, setStatusMessage] = useState('Start a round to begin.')
  const [feedbackTone, setFeedbackTone] = useState<FeedbackTone>('info')
  const [points, setPoints] = useState(0)
  const [winningStreak, setWinningStreak] = useState(0)
  const [highScore, setHighScore] = useState(() => loadHighScore(HIGH_SCORE_KEY))
  const [roundsPlayed, setRoundsPlayed] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [timeLeft, setTimeLeft] = useState(0)
  const [rateLimitDebug, setRateLimitDebug] = useState<RateLimitDebug | null>(null)
  const [revealedClues, setRevealedClues] = useState<string[]>([])
  const [debugCallCount, setDebugCallCount] = useState(0)
  const [debugLastUpdated, setDebugLastUpdated] = useState('-')
  const [debugPulseTick, setDebugPulseTick] = useState(0)
  const expiryHandledRef = useRef(false)

  useEffect(() => {
    if (!round || round.roundStatus !== 'in-progress') {
      return
    }

    const timer = window.setInterval(() => {
      const expires = new Date(round.expiresAtUtc).getTime()
      const diff = Math.max(0, Math.floor((expires - Date.now()) / 1000))
      setTimeLeft(diff)
    }, 250)

    return () => window.clearInterval(timer)
  }, [round])

  // Automatically give up when the timer expires
  useEffect(() => {
    if (timeLeft !== 0 || round?.roundStatus !== 'in-progress' || loading || expiryHandledRef.current) {
      return
    }

    expiryHandledRef.current = true
    const roundId = round.roundId
    let cancelled = false

    setLoading(true)
    api
      .giveUp(roundId)
      .then((result) => {
        if (cancelled) return
        const data = result.data
        setRound((current) => {
          if (!current) return current
          return { ...current, roundStatus: data.roundStatus, answer: data.correctAnswer }
        })
        if (result.rateLimit) setRateLimitDebug(result.rateLimit)
        setDebugCallCount((n) => n + 1)
        setDebugLastUpdated(new Date().toLocaleTimeString())
        setDebugPulseTick((n) => n + 1)
        setStatusMessage(`Time's up! The answer was: ${data.correctAnswer ?? 'unknown'}`)
        setFeedbackTone('warning')
        setWinningStreak(0)
        setGuess('')
      })
      .catch(() => {
        // Silently fail — the round may have already been handled server-side
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [timeLeft, round?.roundStatus, round?.roundId, loading])

  const canInteract = useMemo(
    () => Boolean(round && round.roundStatus === 'in-progress' && !loading),
    [round, loading],
  )

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
      const createdResult = await api.createRound()
      const created = createdResult.data
      setRound({
        roundId: created.roundId,
        clue: created.clue,
        clueIndex: created.clueIndex,
        maxClues: created.maxClues,
        expiresAtUtc: created.expiresAtUtc,
        scoreIfCorrect: created.scoreIfCorrect,
        roundStatus: 'in-progress',
        answer: null,
      })
      setRevealedClues([created.clue])
      refreshDebug(createdResult.rateLimit ?? created.rateLimit)

      setRoundsPlayed((current) => current + 1)
      setGuess('')
      setStatusMessage('Round started. Read the clue and submit your best guess.')
      setFeedbackTone('info')
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

    setLoading(true)
    setError('')

    try {
      const guessResult = await api.guess(round.roundId, guess)
      const result = guessResult.data
      if (result.isCorrect) {
        setPoints((current) => {
          const next = current + result.awardedPoints
          setHighScore((hs) => {
            const updated = Math.max(hs, next)
            saveHighScore(HIGH_SCORE_KEY, updated)
            return updated
          })
          return next
        })
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

      if (result.roundStatus === 'won') {
        setStatusMessage(`Correct! +${result.awardedPoints} points.`)
        setFeedbackTone('success')
        setWinningStreak((current) => current + 1)
      } else if (result.roundStatus === 'lost') {
        setStatusMessage(`No more clues. Answer: ${result.correctAnswer ?? 'unknown'}`)
        setFeedbackTone('warning')
        setWinningStreak(0)
      } else if (result.roundStatus === 'expired') {
        setStatusMessage('Round expired. Start a new round.')
        setFeedbackTone('warning')
        setWinningStreak(0)
      } else {
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

      setStatusMessage(`You gave up. Answer: ${result.correctAnswer ?? 'unknown'}`)
      setFeedbackTone('warning')
      setWinningStreak(0)
      setGuess('')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to give up this round.'
      setError(message)
      setFeedbackTone('error')
    } finally {
      setLoading(false)
    }
  }

  const scoreIfCorrect = round?.scoreIfCorrect ?? 0
  const clueStep = round ? `${round.clueIndex + 1}/${round.maxClues}` : '0/0'
  const usedRequests = rateLimitDebug ? Math.max(0, rateLimitDebug.limit - rateLimitDebug.limitRemaining) : null

  return (
    <section className="panel grid">
      <div className="stats">
        <article className="kpi">
          <p className="kpi-label">Total Points</p>
          <p className="kpi-value">{points}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">High Score</p>
          <p className="kpi-value">{highScore}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Streak 🔥</p>
          <p className="kpi-value">{winningStreak}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Rounds Played</p>
          <p className="kpi-value">{roundsPlayed}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Time Left</p>
          <p className="kpi-value">{round ? `${timeLeft}s` : '-'}</p>
        </article>
      </div>

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

      <p className={`status status-${feedbackTone} ${round?.roundStatus === 'won' ? 'status-win' : ''}`} role="status" aria-live="polite">
        <strong>Status:</strong> {statusMessage}
      </p>
      {round?.roundStatus === 'won' ? <p className="victory-banner">Word cracked! Streak: {winningStreak} 🔥</p> : null}
      {error ? <p className="error">{error}</p> : null}
      <p className="footer">Points available now: {scoreIfCorrect}</p>

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

// ─── Translation Quiz Component ───────────────────────────────────────────────

function TranslationQuiz() {
  const [round, setRound] = useState<ActiveQuizRound | null>(null)
  const [targetLanguage, setTargetLanguage] = useState('de')
  const [statusMessage, setStatusMessage] = useState('Select a target language and start a round.')
  const [feedbackTone, setFeedbackTone] = useState<FeedbackTone>('info')
  const [points, setPoints] = useState(0)
  const [winningStreak, setWinningStreak] = useState(0)
  const [highScore, setHighScore] = useState(() => loadHighScore(QUIZ_HIGH_SCORE_KEY))
  const [roundsPlayed, setRoundsPlayed] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [timeLeft, setTimeLeft] = useState(0)
  const [rateLimitDebug, setRateLimitDebug] = useState<RateLimitDebug | null>(null)
  const [debugCallCount, setDebugCallCount] = useState(0)
  const [debugLastUpdated, setDebugLastUpdated] = useState('-')
  const [debugPulseTick, setDebugPulseTick] = useState(0)
  const expiryHandledRef = useRef(false)

  useEffect(() => {
    if (!round || round.roundStatus !== 'in-progress') {
      return
    }

    const timer = window.setInterval(() => {
      const expires = new Date(round.expiresAtUtc).getTime()
      const diff = Math.max(0, Math.floor((expires - Date.now()) / 1000))
      setTimeLeft(diff)
    }, 250)

    return () => window.clearInterval(timer)
  }, [round])

  // Reveal answer when timer expires
  useEffect(() => {
    if (timeLeft !== 0 || round?.roundStatus !== 'in-progress' || loading || expiryHandledRef.current) {
      return
    }

    expiryHandledRef.current = true
    const roundId = round.roundId
    let cancelled = false

    setLoading(true)
    api
      .submitQuizAnswer(roundId, '__expired__')
      .then((result) => {
        if (cancelled) return
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
        setWinningStreak(0)
      })
      .catch(() => {
        if (cancelled) return
        setRound((current) => {
          if (!current) return current
          return { ...current, roundStatus: 'expired' }
        })
        setStatusMessage("Time's up!")
        setFeedbackTone('warning')
        setWinningStreak(0)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [timeLeft, round?.roundStatus, round?.roundId, loading])

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
      const result = await api.createQuizRound(targetLanguage)
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
      refreshDebug(result.rateLimit ?? created.rateLimit)
      setRoundsPlayed((current) => current + 1)
      setStatusMessage(`Choose the correct ${SUPPORTED_LANGUAGES[created.targetLanguage] ?? created.targetLanguage} translation.`)
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
        setPoints((current) => {
          const next = current + data.awardedPoints
          setHighScore((hs) => {
            const updated = Math.max(hs, next)
            saveHighScore(QUIZ_HIGH_SCORE_KEY, updated)
            return updated
          })
          return next
        })
        setWinningStreak((current) => current + 1)
      } else {
        setWinningStreak(0)
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

      if (data.roundStatus === 'won') {
        setStatusMessage(`Correct! +${data.awardedPoints} point. The translation is: ${data.correctAnswer}`)
        setFeedbackTone('success')
      } else if (data.roundStatus === 'expired') {
        setStatusMessage(`Time's up! The correct answer was: ${data.correctAnswer}`)
        setFeedbackTone('warning')
      } else {
        setStatusMessage(`Incorrect. The correct answer was: ${data.correctAnswer}`)
        setFeedbackTone('warning')
      }
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
        <label className="quiz-lang-label" htmlFor="target-language">
          Translate to:
        </label>
        <select
          id="target-language"
          className="quiz-lang-select"
          value={targetLanguage}
          onChange={(e) => setTargetLanguage(e.target.value)}
          disabled={loading}
        >
          {Object.entries(SUPPORTED_LANGUAGES).map(([code, name]) => (
            <option key={code} value={code}>
              {name} ({code})
            </option>
          ))}
        </select>
      </div>

      <div className="stats">
        <article className="kpi">
          <p className="kpi-label">Total Points</p>
          <p className="kpi-value">{points}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">High Score</p>
          <p className="kpi-value">{highScore}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Streak 🔥</p>
          <p className="kpi-value">{winningStreak}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Rounds Played</p>
          <p className="kpi-value">{roundsPlayed}</p>
        </article>
        <article className="kpi">
          <p className="kpi-label">Time Left</p>
          <p className="kpi-value">{round ? `${timeLeft}s` : '-'}</p>
        </article>
      </div>

      <div className="quiz-word-panel">
        {round ? (
          <>
            <p className="quiz-word-label">What is the {SUPPORTED_LANGUAGES[round.targetLanguage] ?? round.targetLanguage} translation of:</p>
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
      {round?.roundStatus === 'won' ? <p className="victory-banner">Correct! Streak: {winningStreak} 🔥</p> : null}
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
            ? 'Guess the hidden English word from lexical clues generated with Fluky Search.'
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
