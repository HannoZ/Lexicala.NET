namespace Lexicala.NET.Response
{
    /// <summary>
    /// Rate limit info. 
    /// </summary>
    /// <remarks>
    /// The rate limit info is returned as response headers on each response.
    /// </remarks>
    public class RateLimits
    {
        /// <summary>
        /// Gets or sets the daily limit (based on your subscription).
        /// </summary>
        public int DailyLimit { get; set; }
        /// <summary>
        /// Gets or sets the remaining amount of allowed api calls for today.
        /// </summary>
        public int DailyLimitRemaining { get; set; }
        /// <summary>
        /// Gets or sets the throttle limit.
        /// </summary>
        public int Limit { get; set; }
        /// <summary>
        /// Gets or sets the remaining calls before throttling is applied.
        /// </summary>
        public int Remaining { get; set; }
    }
}
