using System;

namespace Lexicala.NET.Demo.Api.Game;

public sealed record CreateRoundResponse(
    Guid RoundId,
    DateTimeOffset ExpiresAtUtc,
    int ClueIndex,
    string Clue,
    int ScoreIfCorrect,
    int MaxClues,
    int RoundSeconds
);

public sealed record NextClueResponse(
    Guid RoundId,
    DateTimeOffset ExpiresAtUtc,
    int ClueIndex,
    string Clue,
    int ScoreIfCorrect,
    int MaxClues
);

public sealed record GuessRequest(string Guess);

public sealed record GuessResponse(
    Guid RoundId,
    bool IsCorrect,
    string RoundStatus,
    int AwardedPoints,
    int CurrentClueIndex,
    string? CorrectAnswer,
    string? Message
);
