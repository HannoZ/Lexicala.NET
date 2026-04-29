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

        /// <summary>
        /// The number of seconds until the request quota resets.
        /// </summary>
        public const string HeaderRateLimitReset = "X-RateLimit-requests-Reset";

        /// <summary>
        /// The hard-limit quota for free plans.
        /// </summary>
        public const string HeaderRapidFreePlanHardLimitLimit = "X-RateLimit-rapid-free-plans-hard-limit-Limit";

        /// <summary>
        /// The remaining hard-limit quota for free plans.
        /// </summary>
        public const string HeaderRapidFreePlanHardLimitRemaining = "X-RateLimit-rapid-free-plans-hard-limit-Remaining";

        /// <summary>
        /// The hard-limit reset time for free plans.
        /// </summary>
        public const string HeaderRapidFreePlanHardLimitReset = "X-RateLimit-rapid-free-plans-hard-limit-Reset";
    }
}
