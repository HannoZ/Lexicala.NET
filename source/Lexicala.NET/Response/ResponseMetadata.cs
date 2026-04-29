namespace Lexicala.NET.Response
{
    /// <summary>
    /// Represents metadata extracted from HTTP response headers.
    /// </summary>
    /// <remarks>
    /// This metadata is populated by <see cref="LexicalaClient"/> for response types returned by
    /// endpoint methods such as search, entry retrieval, and sense retrieval.
    /// </remarks>
    public class ResponseMetadata
    {
        /// <summary>
        /// Gets or sets the ETag value from the response headers.
        /// Use this value for conditional requests by passing it back as <c>If-None-Match</c>
        /// in subsequent calls.
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets parsed rate limit metadata from response headers.
        /// Values may be <c>-1</c> when headers are missing or not parseable.
        /// </summary>
        public RateLimits RateLimits { get; set; }
    }
}

