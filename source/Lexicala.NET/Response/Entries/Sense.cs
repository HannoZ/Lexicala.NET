using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Sense
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("definition")]
        public string Definition { get; set; }

        [JsonPropertyName("range_of_application")]
        public string RangeOfApplication { get; set; }

        [JsonPropertyName("antonyms")]

        public string[] Antonyms { get; set; } = [];

        [JsonPropertyName("synonyms")]
        public string[] Synonyms { get; set; } = [];

        [JsonPropertyName("translations")]
        public Dictionary<string, TranslationObject> Translations { get; set; } = [];

        [JsonPropertyName("examples")] 
        public Example[] Examples { get; set; } = [];

        [JsonPropertyName("compositional_phrases")]
        public CompositionalPhrase[] CompositionalPhrases { get; set; } = [];
    
        [JsonPropertyName("semantic_subcategory")]
        public string SemanticSubcategory { get; set; }

        [JsonPropertyName("geographical_usage")]
        public string GeographicalUsage { get; set; }

        public ResponseMetadata Metadata { get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
