namespace Lexicala.NET.Response
{
    /// <summary>
    /// Represents rate limit values returned by Lexicala response headers.
    /// </summary>
    /// <remarks>
    /// These values are extracted from response headers by <see cref="LexicalaClient"/>.
    /// If a header is missing or unparseable, the corresponding value can be <c>-1</c>.
    /// </remarks>
    public class RateLimits
    {
        /// <summary>
        /// Gets or sets the total request limit for the active billing window.
        /// </summary>
        /// <remarks>
        /// This is a static value indicating the number of requests the plan you are currently subscribed to allows you to make before incurring overages.
        /// </remarks>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the remaining number of allowed API calls in the current billing window.
        /// </summary>
        /// <remarks>
        /// The number of requests remaining (from your plan) before you reach the limit of requests your application is allowed to make. When this reaches zero, you will begin experiencing overage charges. This will reset each day or each month, depending on how the API pricing plan is configured. You can view these limits and quotas on the pricing page of the API in the API Hub.
        /// </remarks>
        public int LimitRemaining { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds until the rate limit window resets.
        /// </summary>
        /// <remarks>
        /// <p>Indicates the number of seconds until the quota resets. This number of seconds would at most be as long as either a day or a month, depending on how the plan was configured.</p>
        /// <p>
        /// Additionally, for every billing object defined by the API provider that is attached to a plan and a set of endpoints in it, there will be three response headers that are similar in purpose to the three x-ratelimit headers described above. For example, if you have a billing object named My-Object-1, the following three response headers would be sent:
        ///
        /// <list type="bullet">
        /// <item>x-ratelimit-my-object-1-limit</item>
        /// <item>x-ratelimit-my-object-1-remaining</item>
        /// <item>x-ratelimit-my-object-1-reset</item>
        /// </list>
        /// </p>
        /// </remarks>
        public long Reset { get; set; }
    }
}

