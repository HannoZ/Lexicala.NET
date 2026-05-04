using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lexicala.NET.Demo.Api.Game;

public sealed class TranslationQuizGameService : ITranslationQuizGameService
{
    private const string LanguagesCacheKey = "translation-quiz-languages";
    private const string DistractorPoolCacheKeyPrefix = "translation-quiz-distractors:";
    private const int MaxGenerationAttempts = 8;
    private const int QuizRoundSeconds = 30;
    private const int ChoiceCount = 4;
    private const int MaxDistractorPoolSize = 24;
    // Keeps rounds accessible for a short window after the game timer elapses so
    // that /expire and late /answer calls succeed even when the client sends the
    // request a few seconds after the server-side deadline.
    private static readonly TimeSpan RoundCacheGracePeriod = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ExpireEarlyTolerance = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan DistractorPoolCacheDuration = TimeSpan.FromMinutes(30);

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

    public async Task<CreateQuizRoundResponse> CreateRoundAsync(string? targetLanguage = null, CancellationToken cancellationToken = default)
    {
        var availableLanguages = await GetTargetLanguagesAsync(cancellationToken);

        string resolvedLanguage;
        if (string.IsNullOrWhiteSpace(targetLanguage))
        {
            if (availableLanguages.Length == 0)
            {
                throw new InvalidOperationException("No target languages are available. Try again later.");
            }

            resolvedLanguage = availableLanguages[Random.Shared.Next(availableLanguages.Length)];
        }
        else
        {
            resolvedLanguage = targetLanguage.Trim().ToLowerInvariant();
        }

        for (var attempt = 1; attempt <= MaxGenerationAttempts; attempt++)
        {
            var generated = await TryGenerateRoundAsync(resolvedLanguage, cancellationToken);
            if (generated is null)
            {
                _logger.LogDebug("Translation Quiz generation attempt {Attempt} failed quality filters", attempt);
                continue;
            }

            var roundId = Guid.NewGuid();
            _cache.Set(roundId, generated, generated.ExpiresAtUtc + RoundCacheGracePeriod);

            return new CreateQuizRoundResponse(
                roundId,
                generated.SourceWord,
                "en",
                resolvedLanguage,
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
        _cache.Set(roundId, round, round.ExpiresAtUtc + RoundCacheGracePeriod);

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

    public Task<QuizAnswerResponse> ExpireRoundAsync(Guid roundId, CancellationToken cancellationToken = default)
    {
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
        _cache.Set(roundId, round, round.ExpiresAtUtc + RoundCacheGracePeriod);

        // Allow a small tolerance for client-side timer rounding; reject if called well before expiry
        var isNearOrPastExpiry = DateTimeOffset.UtcNow >= round.ExpiresAtUtc - ExpireEarlyTolerance;
        if (!isNearOrPastExpiry)
        {
            return Task.FromResult(new QuizAnswerResponse(
                roundId,
                false,
                "lost",
                0,
                round.CorrectAnswer,
                "Round expiry requested before the timer has elapsed."));
        }

        return Task.FromResult(new QuizAnswerResponse(
            roundId,
            false,
            "expired",
            0,
            round.CorrectAnswer,
            "Time's up!"));
    }

    public async Task<string[]> GetTargetLanguagesAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(LanguagesCacheKey, out string[]? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var languages = await _lexicalaClient.LanguagesAsync(cancellationToken);

            // Prefer explicit target_languages; fall back to source_languages excluding "en"
            var targetLanguages = (languages.Resources?.Global?.TargetLanguages is { Length: > 0 } tl ? tl
                : languages.Resources?.Global?.SourceLanguages ?? [])
                .Select(l => l.Trim().ToLowerInvariant())
                .Where(l => l.Length > 0 && l != "en")
                .Distinct()
                .OrderBy(l => l)
                .ToArray();

            if (targetLanguages.Length > 0)
            {
                _cache.Set(LanguagesCacheKey, targetLanguages, TimeSpan.FromHours(1));
            }

            return targetLanguages;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch languages from API; using fallback list");
            return ["de", "nl", "fr", "es"];
        }
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

        var distractors = await GetDistractorsAsync(targetLanguage, correctTranslation, cancellationToken);

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
            RateLimit = GameServiceHelpers.BuildRateLimit(entry.Metadata) ?? GameServiceHelpers.BuildRateLimit(fluky.Metadata)
        };
    }

    private async Task<List<string>> GetDistractorsAsync(string targetLanguage, string correctTranslation, CancellationToken cancellationToken)
    {
        var poolKey = GetDistractorPoolCacheKey(targetLanguage);
        var pool = _cache.TryGetValue(poolKey, out string[]? cachedPool) && cachedPool is not null
            ? cachedPool
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxDistractorPoolSize)
                .ToList()
            : [];

        var distractors = PickDistractors(pool, correctTranslation);
        if (distractors.Count == ChoiceCount - 1)
        {
            return distractors;
        }

        var missingCount = ChoiceCount - 1 - distractors.Count;
        var fetchedDistractors = await FetchDistractorCandidatesAsync(targetLanguage, missingCount, cancellationToken);
        if (fetchedDistractors.Count > 0)
        {
            foreach (var candidate in fetchedDistractors)
            {
                if (pool.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                pool.Add(candidate);
            }

            var persistedPool = pool
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxDistractorPoolSize)
                .ToArray();

            _cache.Set(poolKey, persistedPool, DistractorPoolCacheDuration);
            distractors = PickDistractors(persistedPool, correctTranslation);
        }

        return distractors;
    }

    private async Task<List<string>> FetchDistractorCandidatesAsync(string targetLanguage, int count, CancellationToken cancellationToken)
    {
        if (count <= 0)
        {
            return [];
        }

        var distractorTasks = Enumerable.Range(0, count)
            .Select(_ => _lexicalaClient.FlukySearchAsync(Sources.Global, targetLanguage, cancellationToken: cancellationToken))
            .ToArray();

        var distractorResults = await Task.WhenAll(distractorTasks);

        return distractorResults
            .Select(r => GetFirstHeadword(r.Results.FirstOrDefault()))
            .OfType<string>()
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> PickDistractors(IEnumerable<string> pool, string correctTranslation)
    {
        return pool
            .Where(candidate => !string.Equals(candidate, correctTranslation, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => Random.Shared.Next())
            .Take(ChoiceCount - 1)
            .ToList();
    }

    private static string GetDistractorPoolCacheKey(string targetLanguage) => $"{DistractorPoolCacheKeyPrefix}{targetLanguage}";

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
