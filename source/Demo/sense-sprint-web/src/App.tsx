import { useEffect, useMemo, useState } from 'react'
import './App.css'

type RoundStatus = 'in-progress' | 'won' | 'lost' | 'expired' | 'completed'

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

type CreateRoundResponse = {
  roundId: string
  expiresAtUtc: string
  clueIndex: number
  clue: string
  scoreIfCorrect: number
  maxClues: number
  roundSeconds: number
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

const api = {
  async createRound(): Promise<CreateRoundResponse> {
    const response = await fetch('/game/sense-sprint/rounds', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    return parseResponse<CreateRoundResponse>(response)
  },

  async nextClue(roundId: string): Promise<NextClueResponse> {
    const response = await fetch(`/game/sense-sprint/rounds/${roundId}/clues/next`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    return parseResponse<NextClueResponse>(response)
  },

  async guess(roundId: string, guessText: string): Promise<GuessResponse> {
    const response = await fetch(`/game/sense-sprint/rounds/${roundId}/guess`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ guess: guessText }),
    })

    return parseResponse<GuessResponse>(response)
  },
}

async function parseResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorPayload = (await response.json().catch(() => null)) as ProblemResponse | null
    const detail = errorPayload?.detail ?? errorPayload?.title ?? 'Request failed'
    throw new Error(detail)
  }

  return (await response.json()) as T
}

function App() {
  const [round, setRound] = useState<ActiveRound | null>(null)
  const [guess, setGuess] = useState('')
  const [statusMessage, setStatusMessage] = useState('Start a round to begin.')
  const [points, setPoints] = useState(0)
  const [roundsPlayed, setRoundsPlayed] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [timeLeft, setTimeLeft] = useState(0)

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

  const canInteract = useMemo(
    () => Boolean(round && round.roundStatus === 'in-progress' && !loading),
    [round, loading],
  )

  async function startRound(): Promise<void> {
    setLoading(true)
    setError('')

    try {
      const created = await api.createRound()
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

      setRoundsPlayed((current) => current + 1)
      setGuess('')
      setStatusMessage('Round started. Read the clue and submit your best guess.')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to start round.'
      setError(message)
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
      const next = await api.nextClue(round.roundId)
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
      setStatusMessage('New clue revealed.')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to reveal next clue.'
      setError(message)
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
      const result = await api.guess(round.roundId, guess)
      if (result.isCorrect) {
        setPoints((current) => current + result.awardedPoints)
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

      if (result.roundStatus === 'won') {
        setStatusMessage(`Correct! +${result.awardedPoints} points.`)
      } else if (result.roundStatus === 'lost') {
        setStatusMessage(`No more clues. Answer: ${result.correctAnswer ?? 'unknown'}`)
      } else if (result.roundStatus === 'expired') {
        setStatusMessage('Round expired. Start a new round.')
      } else {
        setStatusMessage('Not correct yet. Ask for the next clue or try again.')
      }

      setGuess('')
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unable to submit guess.'
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  const scoreIfCorrect = round?.scoreIfCorrect ?? 0
  const clueStep = round ? `${round.clueIndex + 1}/${round.maxClues}` : '0/0'

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
        <div className="stats">
          <article className="kpi">
            <p className="kpi-label">Total Points</p>
            <p className="kpi-value">{points}</p>
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
            <button className="button button-ghost" onClick={() => void startRound()} disabled={loading}>
              {round ? 'New Round' : 'Start Round'}
            </button>
          </div>
        </div>

        <p className="status">
          <strong>Status:</strong> {statusMessage}
        </p>
        {error ? <p className="error">{error}</p> : null}
        <p className="footer">Points available now: {scoreIfCorrect}</p>
      </section>
    </main>
  )
}

export default App
