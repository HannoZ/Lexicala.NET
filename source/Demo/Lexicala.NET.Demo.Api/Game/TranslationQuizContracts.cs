using System;

namespace Lexicala.NET.Demo.Api.Game;

public sealed record CreateQuizRoundResponse(
    Guid RoundId,
    string SourceWord,
    string SourceLanguage,
    string TargetLanguage,
    string[] Choices,
    DateTimeOffset ExpiresAtUtc,
    int RoundSeconds,
    RateLimitDebugResponse? RateLimit
);

public sealed record QuizAnswerRequest(string Choice);

public sealed record QuizAnswerResponse(
    Guid RoundId,
    bool IsCorrect,
    string RoundStatus,
    int AwardedPoints,
    string CorrectAnswer,
    string? Message
);
