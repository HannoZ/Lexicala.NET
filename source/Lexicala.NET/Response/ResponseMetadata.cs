namespace Lexicala.NET.Response
{
    /// <summary>
    /// This class contains metadata information that is returned as response headers for each request.
    /// </summary>
    public class ResponseMetadata
    {
        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        public string ETag { get; set; }
        /// <summary>
        /// Gets or sets the rate limits info.
        /// </summary>
        public RateLimits RateLimits { get; set; }
    }
}
