namespace Lexicala.NET.Response
{
    /// <summary>
    /// Contains response header constants.
    /// </summary>
    public static class ResponseHeaders
    {
        /// <summary>
        /// The rate limit limit header.
        /// </summary>
        public const string HeaderRateLimitLimit = "X-RateLimit-Limit";
        /// <summary>
        /// The rate limit remaining header.
        /// </summary>
        public const string HeaderRateLimitRemaining = "X-RateLimit-Remaining";
        /// <summary>
        /// The rate limit daily limit header.
        /// </summary>
        public const string HeaderRateLimitDailyLimit = "X-RateLimit-DailyLimit";
        /// <summary>
        /// The rate limit daily limit remaining header.
        /// </summary>
        public const string HeaderDailyLimitRemaining = "X-RateLimit-DailyLimit-Remaining";
    }
}