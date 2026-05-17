using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Translation
{
    /// <summary>
    /// Represents a translation endpoint response payload with raw result objects.
    /// </summary>
    /// <remarks>
    /// Translation endpoint result shapes can vary by endpoint and source resource.
    /// Raw <see cref="JsonElement"/> values are exposed so callers can parse as needed.
    /// </remarks>
    public class TranslationResponse
    {
        /// <summary>
        /// Gets or sets the total number of matching results.
        /// </summary>
        [JsonPropertyName("n_results")]
        public int NResults { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        [JsonPropertyName("page_number")]
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of results per page.
        /// </summary>
        [JsonPropertyName("results_per_page")]
        public int ResultsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        [JsonPropertyName("n_pages")]
        public int NPages { get; set; }

        /// <summary>
        /// Gets or sets the number of pages available for retrieval.
        /// </summary>
        [JsonPropertyName("available_n_pages")]
        public int AvailableNPages { get; set; }

        /// <summary>
        /// Gets or sets raw endpoint result objects.
        /// </summary>
        [JsonPropertyName("results")]
        public JsonElement[] Results { get; set; } = [];

        /// <summary>
        /// Gets or sets response header metadata (ETag and rate limits).
        /// </summary>
        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
    }
}
