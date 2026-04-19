using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
    public class Sense
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("definition")]
        public string Definition { get; set; }
    }
}
