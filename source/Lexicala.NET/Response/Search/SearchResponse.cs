using Newtonsoft.Json;

namespace Lexicala.NET.Response.Search
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class SearchResponse
    {
        [JsonProperty("n_results")]
        public int NResults { get; set; }

        [JsonProperty("page_number")]
        public int PageNumber { get; set; }

        [JsonProperty("results_per_page")]
        public int ResultsPerPage { get; set; }

        [JsonProperty("n_pages")]
        public int NPages { get; set; }

        [JsonProperty("available_n_pages")]
        public int AvailableNPages { get; set; }

        [JsonProperty("results")] 
        public Result[] Results { get; set; } = { };

        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
