using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lexicala.NET.Demo.Api.Game;

public sealed class TranslationQuizGameService : ITranslationQuizGameService
{
    private static readonly string[] SupportedTargetLanguages = ["de", "nl", "fr", "es"];
    private const int MaxGenerationAttempts = 8;
    private const int QuizRoundSeconds = 30;
    private const int ChoiceCount = 4;

    private readonly ILexicalaClient _lexicalaClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TranslationQuizGameService> _logger;

    public TranslationQuizGameService(
        ILexicalaClient lexicalaClient,
        IMemoryCache cache,
        ILogger<TranslationQuizGameService> logger)
    {
        _lexicalaClient = lexicalaClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CreateQuizRoundResponse> CreateRoundAsync(string targetLanguage = "de", CancellationToken cancellationToken = default)
    {
        if (!SupportedTargetLanguages.Contains(targetLanguage, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Target language '{targetLanguage}' is not supported. Supported: {string.Join(", ", SupportedTargetLanguages)}.");
        }

        for (var attempt = 1; attempt <= MaxGenerationAttempts; attempt++)
        {
            var generated = await TryGenerateRoundAsync(targetLanguage, cancellationToken);
            if (generated is null)
            {
                _logger.LogDebug("Translation Quiz generation attempt {Attempt} failed quality filters", attempt);
                continue;
            }

            var roundId = Guid.NewGuid();
            _cache.Set(roundId, generated, generated.ExpiresAtUtc);

            return new CreateQuizRoundResponse(
                roundId,
                generated.SourceWord,
                "en",
                targetLanguage,
                generated.Choices,
                generated.ExpiresAtUtc,
                QuizRoundSeconds,
                generated.RateLimit);
        }

        throw new InvalidOperationException("Could not generate a playable round. Try again.");
    }

    public Task<QuizAnswerResponse> SubmitAnswerAsync(Guid roundId, string choice, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(choice);

        var round = GetRequiredRound(roundId);

        if (round.IsCompleted)
        {
            return Task.FromResult(new QuizAnswerResponse(
                roundId,
                false,
                "completed",
                0,
                round.CorrectAnswer,
                "Round already completed. Start a new round."));
        }

        round.IsCompleted = true;
        _cache.Set(roundId, round, round.ExpiresAtUtc);

        var isExpired = DateTimeOffset.UtcNow > round.ExpiresAtUtc;
        if (isExpired)
        {
            return Task.FromResult(new QuizAnswerResponse(
                roundId,
                false,
                "expired",
                0,
                round.CorrectAnswer,
                "Time's up! Start a new round."));
        }

        var isCorrect = string.Equals(round.CorrectAnswer, choice.Trim(), StringComparison.OrdinalIgnoreCase);
        if (isCorrect)
        {
            return Task.FromResult(new QuizAnswerResponse(
                roundId,
                true,
                "won",
                1,
                round.CorrectAnswer,
                "Correct!"));
        }

        return Task.FromResult(new QuizAnswerResponse(
            roundId,
            false,
            "lost",
            0,
            round.CorrectAnswer,
            $"Incorrect. The correct translation was: {round.CorrectAnswer}"));
    }

    private async Task<TranslationQuizRoundState?> TryGenerateRoundAsync(string targetLanguage, CancellationToken cancellationToken)
    {
        // Get a random English word via FlukySearch
        var fluky = await _lexicalaClient.FlukySearchAsync(Sources.Global, "en", cancellationToken: cancellationToken);
        var candidate = fluky.Results.FirstOrDefault();
        if (candidate?.Id is null)
        {
            return null;
        }

        // Fetch the full entry to access its translations
        var entry = await _lexicalaClient.GetEntryAsync(candidate.Id, cancellationToken: cancellationToken);
        var sourceWord = entry.Headwords.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(sourceWord))
        {
            return null;
        }

        // Find a translation in the target language from any sense
        var correctTranslation = FindTranslation(entry, targetLanguage);
        if (string.IsNullOrWhiteSpace(correctTranslation))
        {
            return null;
        }

        // Fetch distractors in parallel: FlukySearch in the target language, use headwords as wrong choices
        var distractorTasks = Enumerable.Range(0, ChoiceCount - 1)
            .Select(_ => _lexicalaClient.FlukySearchAsync(Sources.Global, targetLanguage, cancellationToken: cancellationToken))
            .ToArray();

        var distractorResults = await Task.WhenAll(distractorTasks);

        var distractors = distractorResults
            .Select(r => GetFirstHeadword(r.Results.FirstOrDefault()))
            .OfType<string>()
            .Where(d => !string.IsNullOrWhiteSpace(d) && !string.Equals(d, correctTranslation, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(ChoiceCount - 1)
            .ToList();

        if (distractors.Count < ChoiceCount - 1)
        {
            return null;
        }

        // Build shuffled choices (1 correct + 3 distractors)
        var choices = distractors
            .Append(correctTranslation)
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();

        return new TranslationQuizRoundState
        {
            SourceWord = sourceWord,
            CorrectAnswer = correctTranslation,
            Choices = choices,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(QuizRoundSeconds),
            IsCompleted = false,
            RateLimit = BuildRateLimit(entry.Metadata) ?? BuildRateLimit(fluky.Metadata)
        };
    }

    private static string? FindTranslation(Lexicala.NET.Response.Entries.Entry entry, string targetLanguage)
    {
        foreach (var sense in entry.Senses)
        {
            if (sense.Translations.TryGetValue(targetLanguage, out var translationObj))
            {
                var text = translationObj.Translation?.Text
                    ?? translationObj.Translations?.FirstOrDefault()?.Text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private static string? GetFirstHeadword(Lexicala.NET.Response.Search.Result? result)
    {
        if (result is null)
        {
            return null;
        }

        var hw = result.Headword;
        return hw.HeadwordElementArray is { Length: > 0 }
            ? hw.HeadwordElementArray[0].Text
            : hw.Headword?.Text;
    }

    private static RateLimitDebugResponse? BuildRateLimit(ResponseMetadata? metadata)
    {
        var limits = metadata?.RateLimits;
        if (limits is null)
        {
            return null;
        }

        if (limits.Limit < 0 || limits.LimitRemaining < 0 || limits.Reset < 0)
        {
            return null;
        }

        return new RateLimitDebugResponse(limits.Limit, limits.LimitRemaining, limits.Reset);
    }

    private TranslationQuizRoundState GetRequiredRound(Guid roundId)
    {
        if (!_cache.TryGetValue(roundId, out TranslationQuizRoundState? round) || round is null)
        {
            throw new KeyNotFoundException("Round not found. Start a new round.");
        }

        return round;
    }

    private sealed class TranslationQuizRoundState
    {
        public required string SourceWord { get; init; }

        public required string CorrectAnswer { get; init; }

        public required string[] Choices { get; init; }

        public required DateTimeOffset ExpiresAtUtc { get; init; }

        public bool IsCompleted { get; set; }

        public RateLimitDebugResponse? RateLimit { get; init; }
    }
}
