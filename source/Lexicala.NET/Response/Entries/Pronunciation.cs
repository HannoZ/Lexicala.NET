using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    public class Pronunciation
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
    
}
