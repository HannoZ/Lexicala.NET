using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lexicala.NET.ConsoleApp.Game;

public interface ISenseSprintGameService
{
    Task<CreateRoundResponse> CreateRoundAsync(CancellationToken cancellationToken = default);

    Task<NextClueResponse> RevealNextClueAsync(Guid roundId, CancellationToken cancellationToken = default);

    Task<GuessResponse> SubmitGuessAsync(Guid roundId, string guess, CancellationToken cancellationToken = default);
}
