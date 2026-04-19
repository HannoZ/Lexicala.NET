using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
    public class Headword
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("pos")]
        public string Pos { get; set; }
    }
}
