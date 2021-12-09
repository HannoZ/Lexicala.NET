using Newtonsoft.Json;

namespace Lexicala.NET.Response.Search
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}