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
        /// Gets or sets the limit (based on your subscription).
        /// </summary>
        /// <remarks>
        /// This is a static value indicating the number of requests the plan you are currently subscribed to allows you to make before incurring overages.
        /// </remarks>
        public int Limit { get; set; }
        /// <summary>
        /// Gets or sets the remaining amount of allowed api calls.
        /// </summary>
        /// <remarks>
        /// The number of requests remaining (from your plan) before you reach the limit of requests your application is allowed to make. When this reaches zero, you will begin experiencing overage charges. This will reset each day or each month, depending on how the API pricing plan is configured. You can view these limits and quotas on the pricing page of the API in the API Hub.
        /// </remarks>
        public int LimitRemaining { get; set; }
        /// <summary>
        /// Gets or sets the reset time of the limit in seconds.
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
