using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

