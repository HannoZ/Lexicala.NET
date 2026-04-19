using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Inflection
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("tense")]
        public string Tense { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
