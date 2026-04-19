using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    public class Example
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("translations")]
        public Dictionary<string, TranslationObject> Translations { get; set; }
    }
}
