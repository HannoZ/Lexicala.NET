using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class CompositionalPhrase
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("definition")]
        public string Definition { get; set; }

        [JsonPropertyName("semantic_subcategory")]
        public string SemanticSubcategory { get; set; }

        [JsonPropertyName("senses")]
        public Sense[] Senses { get; set; } = [];

        [JsonPropertyName("translations")]
        public Dictionary<string, TranslationObject> Translations { get; set; }

        [JsonPropertyName("examples")]
        public Example[] Examples { get; set; } = [];

        [JsonPropertyName("semantic_category")]
        public string SemanticCategory { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
