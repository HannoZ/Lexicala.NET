namespace Lexicala.NET.Response
{
    /// <summary>
    /// Contains response header constants.
    /// </summary>
    public static class ResponseHeaders
    {
        /// <summary>
        /// The rate limit daily limit header.
        /// </summary>
        public const string HeaderRateLimitRequestsLimit = "X-RateLimit-requests-Limit";
        /// <summary>
        /// The rate limit daily limit remaining header.
        /// </summary>
        public const string HeaderRateLimitRequestsRemaining = "X-RateLimit-requests-Remaining";

        public const string HeaderRateLimitReset = "X-RateLimit-requests-Reset";

        public const string HeaderRapidFreePlanHardLimitLimit = "X-RateLimit-rapid-free-plans-hard-limit-Limit";
        public const string HeaderRapidFreePlanHardLimitRemaining = "X-RateLimit-rapid-free-plans-hard-limit-Remaining";
        public const string HeaderRapidFreePlanHardLimitReset = "X-RateLimit-rapid-free-plans-hard-limit-Reset";
    }
}