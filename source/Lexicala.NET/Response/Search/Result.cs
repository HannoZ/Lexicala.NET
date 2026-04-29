using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Result
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("headword")]
        public HeadwordObject Headword { get; set; }

        [JsonPropertyName("senses")] 
        public Sense[] Senses { get; set; } = [];
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
