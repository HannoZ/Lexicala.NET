namespace Lexicala.NET.Response
{
    /// <summary>
    /// This class contains metadata information that is returned as response headers for each request.
    /// </summary>
    public class ResponseMetadata
    {
        public string ETag { get; set; }
        public RateLimits RateLimits { get; set; }
    }
}
