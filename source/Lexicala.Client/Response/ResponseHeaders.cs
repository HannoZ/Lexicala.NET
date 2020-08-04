namespace Lexicala.NET.Client.Response
{
    public static class ResponseHeaders
    {
        public const string HeaderRateLimitLimit = "X-RateLimit-Limit";
        public const string HeaderRateLimitRemaining = "X-RateLimit-Remaining";
        public const string HeaderRateLimitDailyLimit = "X-RateLimit-DailyLimit";
        public const string HeaderDailyLimitRemaining = "X-RateLimit-DailyLimit-Remaining";
    }
}