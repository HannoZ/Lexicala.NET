using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Languages
{
    public class Resource
    {
        [JsonPropertyName("source_languages")]
        public string[] SourceLanguages { get; set; }

        [JsonPropertyName("target_languages")]
        public string[] TargetLanguages { get; set; }
    }
}
