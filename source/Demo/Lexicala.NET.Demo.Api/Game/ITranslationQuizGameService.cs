using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexicala.NET.Demo.Api.Game;

public interface ITranslationQuizGameService
{
    Task<CreateQuizRoundResponse> CreateRoundAsync(string? targetLanguage = null, CancellationToken cancellationToken = default);

    Task<QuizAnswerResponse> SubmitAnswerAsync(Guid roundId, string choice, CancellationToken cancellationToken = default);

    Task<QuizAnswerResponse> ExpireRoundAsync(Guid roundId, CancellationToken cancellationToken = default);
}
