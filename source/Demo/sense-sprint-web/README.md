# Sense Sprint

A lexical word guessing game powered by the Lexicala API. The web demo includes `Sense Sprint` and `Translation Quiz`.

## Token Usage Warning

`Translation Quiz` consumes substantially more Lexicala API quota than `Sense Sprint`. Each round needs a random source word, a full entry lookup, and several extra lookups to assemble plausible wrong answers. If you are testing with a free evaluation subscription, use game mode 2 with care.

## Features

- **Streak Progression**: Build winning streaks with progressive score multipliers
  - 3 wins: 1.15x scoring bonus
  - 5 wins: 1.35x bonus + major celebration animation
  - 10 wins: 1.65x bonus
  - 15+ wins: 2.00x bonus
- **Persistent Stats**: All-time statistics saved locally including:
  - Current and high score
  - Win/loss record
  - Expiration and give-up counts
  - Words guessed correctly
  - Clues requested and total guesses
  - Average clues per round
  - Current and highest streak
  - Major celebrations unlocked
- **Session Recovery**: Your active round is saved locally, so page reloads resume mid-game
- **Streak Warnings**: Confirm before starting a new word if it will reset your streak
- **Stats Reset**: Clear all saved statistics with one confirmed action (current round preserved)

## Getting Started

### Prerequisites

1. **Backend API Running**: Start the Lexicala.NET.Demo.Api first (see main README for setup)
   ```bash
   dotnet run --project ../Lexicala.NET.Demo.Api/Lexicala.NET.Demo.Api.csproj
   ```

2. **Node.js**: Ensure Node.js is installed (v18+)

### Development

1. Navigate to the frontend folder:
   ```bash
   cd source/Demo/sense-sprint-web
   ```

2. Install dependencies (required once):

   **PowerShell:**
   ```powershell
   npm.cmd install
   ```

   **Bash / Command Prompt:**
   ```bash
   npm install
   ```

3. Start the dev server:

   **PowerShell:**
   ```powershell
   npm.cmd run dev
   ```

   **Bash / Command Prompt:**
   ```bash
   npm run dev
   ```

4. Open in your browser:
   - `http://localhost:5173`

The Vite dev server proxies `/game/*` calls to `http://localhost:5000`, so game endpoints work without extra CORS setup.

**Note:** PowerShell requires `npm.cmd` due to execution policies. If you see "npm is not recognized", ensure you're using `npm.cmd run dev` (PowerShell) or open Command Prompt / Git Bash instead.

## Building for Production

From the `sense-sprint-web` folder:

```bash
npm run build
```

The production build outputs to the `dist/` folder, suitable for serving as a static site.

## Tech Stack

- **React 19**: UI framework
- **TypeScript 6**: Type safety
- **Vite 8**: Build tool and dev server
- **Tailwind-inspired CSS**: Custom responsive styling

## Architecture

The app is a single-component React application (`App.tsx`) with:
- Local state management for game rounds, stats, and UI feedback
- LocalStorage persistence for stats and active sessions
- Automatic timeout detection for expired rounds
- Session hydration on page load with stale-round detection

## Game Rules

1. **Start a Round**: Click "Start Round" to receive the first clue
2. **Make Guesses**: Type your guess and press Enter or click "Submit Guess"
3. **Request Clues**: Click "Next Clue" to reveal additional hints (score penalty applies)
4. **Give Up**: Click "Give Up" to see the answer and move to the next round
5. **Time Limit**: Each round expires after 60 seconds

### Scoring

- Base score starts at 50 points
- Decreases as clues are revealed
- Multiplied by your current streak bonus
- Streak resets on:
  - Incorrect guesses when no more clues are available
  - Round timeout
  - Giving up
  - Starting a new word mid-game

## Statistics Explained

- **Total Points**: Cumulative score across all games
- **High Score**: Best single-round payout (not cumulative)
- **Win Streak**: Current consecutive wins
- **Highest Streak**: Best consecutive wins in this session
- **Games Played**: Total rounds started
- **Wins/Losses**: Correct vs. incorrect final answers
- **Expired/Give Ups**: Rounds lost to timeout or surrender
- **Abandoned Rounds**: Rounds replaced by starting a new word
- **Clues Requested**: Total extra clues revealed
- **Avg. Clues/Round**: Average clues seen per game
- **Avg. Extra Clues/Round**: Average requested clues per game

## Resetting Data

Click the "Reset Stats" button in the streak bonus panel to clear all saved statistics. The current active round is preserved.


