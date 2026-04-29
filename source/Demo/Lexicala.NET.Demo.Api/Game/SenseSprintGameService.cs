using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Lexicala.NET.Response.Entries;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lexicala.NET.Demo.Api.Game;

public sealed class SenseSprintGameService : ISenseSprintGameService
{
    private static readonly int[] ScoresByClueIndex = [4, 3, 2, 1];
    private const int MaxGenerationAttempts = 8;
    private const int RoundSeconds = 60;

    private readonly ILexicalaClient _lexicalaClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SenseSprintGameService> _logger;

    public SenseSprintGameService(
        ILexicalaClient lexicalaClient,
        IMemoryCache cache,
        ILogger<SenseSprintGameService> logger)
    {
        _lexicalaClient = lexicalaClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CreateRoundResponse> CreateRoundAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= MaxGenerationAttempts; attempt++)
        {
            var generated = await TryGenerateRoundAsync(cancellationToken);
            if (generated is null)
            {
                _logger.LogDebug("Sense Sprint generation attempt {Attempt} failed quality filters", attempt);
                continue;
            }

            var roundId = Guid.NewGuid();
            _cache.Set(roundId, generated, generated.ExpiresAtUtc);

            return new CreateRoundResponse(
                roundId,
                generated.ExpiresAtUtc,
                generated.CurrentClueIndex,
                generated.Clues[generated.CurrentClueIndex],
                ScoresByClueIndex[generated.CurrentClueIndex],
                generated.Clues.Count,
                RoundSeconds,
                generated.RateLimit);
        }

        throw new InvalidOperationException("Could not generate a playable round from Fluky Search. Try again.");
    }

    public Task<NextClueResponse> RevealNextClueAsync(Guid roundId, CancellationToken cancellationToken = default)
    {
        var round = GetRequiredRound(roundId);
        EnsureRoundIsActive(roundId, round);

        if (round.CurrentClueIndex >= round.Clues.Count - 1)
        {
            throw new InvalidOperationException("No more clues are available for this round.");
        }

        round.CurrentClueIndex += 1;
        _cache.Set(roundId, round, round.ExpiresAtUtc);

        return Task.FromResult(new NextClueResponse(
            roundId,
            round.ExpiresAtUtc,
            round.CurrentClueIndex,
            round.Clues[round.CurrentClueIndex],
            ScoresByClueIndex[round.CurrentClueIndex],
            round.Clues.Count));
    }

    public Task<GuessResponse> SubmitGuessAsync(Guid roundId, string guess, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guess);

        var round = GetRequiredRound(roundId);

        if (DateTimeOffset.UtcNow > round.ExpiresAtUtc)
        {
            _cache.Remove(roundId);
            return Task.FromResult(new GuessResponse(
                roundId,
                false,
                "expired",
                0,
                round.CurrentClueIndex,
                round.Answer,
                "Round expired. Start a new round."));
        }

        if (round.IsCompleted)
        {
            return Task.FromResult(new GuessResponse(
                roundId,
                false,
                "completed",
                0,
                round.CurrentClueIndex,
                round.Answer,
                "Round already completed. Start a new round."));
        }

        var isCorrect = Normalize(round.Answer) == Normalize(guess);
        if (isCorrect)
        {
            round.IsCompleted = true;
            var points = ScoresByClueIndex[round.CurrentClueIndex];
            _cache.Set(roundId, round, round.ExpiresAtUtc);
            return Task.FromResult(new GuessResponse(
                roundId,
                true,
                "won",
                points,
                round.CurrentClueIndex,
                round.Answer,
                "Correct!"));
        }

        var isOutOfClues = round.CurrentClueIndex >= round.Clues.Count - 1;
        if (isOutOfClues)
        {
            round.IsCompleted = true;
            _cache.Set(roundId, round, round.ExpiresAtUtc);
            return Task.FromResult(new GuessResponse(
                roundId,
                false,
                "lost",
                0,
                round.CurrentClueIndex,
                round.Answer,
                "No clues remaining."));
        }

        return Task.FromResult(new GuessResponse(
            roundId,
            false,
            "in-progress",
            0,
            round.CurrentClueIndex,
            null,
            "Not quite. Ask for the next clue or guess again."));
    }

    public Task<GuessResponse> GiveUpAsync(Guid roundId, CancellationToken cancellationToken = default)
    {
        var round = GetRequiredRound(roundId);

        if (round.IsCompleted)
        {
            throw new InvalidOperationException("Round already completed. Start a new round.");
        }

        round.IsCompleted = true;
        _cache.Set(roundId, round, round.ExpiresAtUtc);

        var isExpired = DateTimeOffset.UtcNow > round.ExpiresAtUtc;
        return Task.FromResult(new GuessResponse(
            roundId,
            false,
            isExpired ? "expired" : "lost",
            0,
            round.CurrentClueIndex,
            round.Answer,
            isExpired ? "Time's up! The answer was revealed." : "Round ended. Better luck next time."));
    }

    private async Task<SenseSprintRoundState?> TryGenerateRoundAsync(CancellationToken cancellationToken)
    {
        var fluky = await _lexicalaClient.FlukySearchAsync(Sources.Global, "en", cancellationToken: cancellationToken);
        var candidate = fluky.Results.FirstOrDefault();
        if (candidate?.Id is null)
        {
            return null;
        }

        var entry = await _lexicalaClient.GetEntryAsync(candidate.Id, cancellationToken: cancellationToken);
        var answer = entry.Headwords.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(answer))
        {
            return null;
        }

        var sense = entry.Senses.FirstOrDefault(HasDefinition);
        if (sense is null)
        {
            return null;
        }

        var clues = BuildClues(entry, sense, answer);
        if (clues.Count < 4)
        {
            return null;
        }

        return new SenseSprintRoundState
        {
            Answer = answer,
            Clues = clues,
            CurrentClueIndex = 0,
            IsCompleted = false,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(RoundSeconds),
            RateLimit = GameServiceHelpers.BuildRateLimit(entry.Metadata) ?? GameServiceHelpers.BuildRateLimit(fluky.Metadata)
        };
    }

    private static List<string> BuildClues(Entry entry, Sense sense, string answer)
    {
        var clues = new List<string>(capacity: 4)
        {
            "Definition: " + MaskAnswer(sense.Definition, answer)
        };

        var relation = sense.Synonyms.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?? sense.Antonyms.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        if (!string.IsNullOrWhiteSpace(relation))
        {
            clues.Add("Related word: " + relation);
        }
        else
        {
            var partOfSpeech = entry.Headwords
                .SelectMany(x => x.PartOfSpeeches)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (!string.IsNullOrWhiteSpace(partOfSpeech))
            {
                clues.Add("Part of speech: " + partOfSpeech);
            }
        }

        var example = sense.Examples
            .Select(x => x.Text)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        if (!string.IsNullOrWhiteSpace(example))
        {
            clues.Add("Example: " + MaskAnswer(example, answer));
        }

        var letterHint = BuildLetterHint(answer);
        clues.Add(letterHint);

        // Keep exactly four clues for stable frontend handling and deterministic scoring.
        while (clues.Count < 4)
        {
            clues.Add(letterHint);
        }

        if (clues.Count > 4)
        {
            clues = clues.Take(4).ToList();
        }

        return clues;
    }

    private static bool HasDefinition(Sense sense) => !string.IsNullOrWhiteSpace(sense.Definition);

    private static string BuildLetterHint(string answer)
    {
        var trimmed = answer.Trim();
        var first = trimmed[0];
        var lettersOnly = new string(trimmed.Where(char.IsLetter).ToArray()).Length;
        return $"Final hint: starts with '{char.ToUpperInvariant(first)}' and has {lettersOnly} letters.";
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static string MaskAnswer(string text, string answer)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            return text;
        }

        var replacement = new string('*', Math.Max(3, answer.Length));
        var escapedAnswer = Regex.Escape(answer.Trim());
        return Regex.Replace(text, escapedAnswer, replacement, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private SenseSprintRoundState GetRequiredRound(Guid roundId)
    {
        if (!_cache.TryGetValue(roundId, out SenseSprintRoundState? round) || round is null)
        {
            throw new KeyNotFoundException("Round not found. Start a new round.");
        }

        return round;
    }

    private void EnsureRoundIsActive(Guid roundId, SenseSprintRoundState round)
    {
        if (DateTimeOffset.UtcNow > round.ExpiresAtUtc)
        {
            _cache.Remove(roundId);
            throw new InvalidOperationException("Round expired. Start a new round.");
        }

        if (round.IsCompleted)
        {
            throw new InvalidOperationException("Round already completed. Start a new round.");
        }
    }

    private sealed class SenseSprintRoundState
    {
        public required string Answer { get; init; }

        public required List<string> Clues { get; init; }

        public required DateTimeOffset ExpiresAtUtc { get; init; }

        public int CurrentClueIndex { get; set; }

        public bool IsCompleted { get; set; }

        public RateLimitDebugResponse? RateLimit { get; init; }
    }
}
