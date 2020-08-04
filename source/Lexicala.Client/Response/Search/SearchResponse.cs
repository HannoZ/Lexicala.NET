using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Search
{
    public class SearchResponse
    {
        [JsonProperty("n_results")]
        public long NResults { get; set; }

        [JsonProperty("page_number")]
        public long PageNumber { get; set; }

        [JsonProperty("results_per_page")]
        public long ResultsPerPage { get; set; }

        [JsonProperty("n_pages")]
        public long NPages { get; set; }

        [JsonProperty("available_n_pages")]
        public long AvailableNPages { get; set; }

        [JsonProperty("results")] 
        public Result[] Results { get; set; } = { };

        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
    }
}
