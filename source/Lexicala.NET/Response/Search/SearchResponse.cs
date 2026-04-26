using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Represents the response payload for search endpoints.
    /// </summary>
    /// <remarks>
    /// Populated by <see cref="ILexicalaClient.BasicSearchAsync"/>,
    /// <see cref="ILexicalaClient.AdvancedSearchAsync"/>,
    /// <see cref="ILexicalaClient.SearchDefinitionsAsync"/>, and
    /// <see cref="ILexicalaClient.FlukySearchAsync"/>.
    /// </remarks>
    public class SearchResponse
    {
        [JsonPropertyName("n_results")]
        public int NResults { get; set; }

        [JsonPropertyName("page_number")]
        public int PageNumber { get; set; }

        [JsonPropertyName("results_per_page")]
        public int ResultsPerPage { get; set; }

        [JsonPropertyName("n_pages")]
        public int NPages { get; set; }

        [JsonPropertyName("available_n_pages")]
        public int AvailableNPages { get; set; }

        [JsonPropertyName("results")] 
        public Result[] Results { get; set; } = { };

        /// <summary>
        /// Gets or sets response header metadata (ETag and rate limits) for this search result.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ResponseMetadata.ETag"/> to issue conditional requests, and
        /// <see cref="ResponseMetadata.RateLimits"/> to observe request quota usage.
        /// </remarks>
        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

