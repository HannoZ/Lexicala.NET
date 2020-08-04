using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Search
{
    public class Result
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("headword")]
        public HeadwordObject Headword { get; set; }

        [JsonProperty("senses")] 
        public Sense[] Senses { get; set; } = { };
    }

}