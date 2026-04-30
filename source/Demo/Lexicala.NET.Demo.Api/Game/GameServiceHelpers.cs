using Lexicala.NET.Response;

namespace Lexicala.NET.Demo.Api.Game;

internal static class GameServiceHelpers
{
    internal static RateLimitDebugResponse? BuildRateLimit(ResponseMetadata? metadata)
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
}
