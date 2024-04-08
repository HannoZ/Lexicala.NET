using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class CompositionalPhrase
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("semantic_subcategory", NullValueHandling = NullValueHandling.Ignore)]
        public string SemanticSubcategory { get; set; }

        [JsonProperty("senses")]
        public Sense[] Senses { get; set; } = [];

        [JsonProperty("translations")]
        public Dictionary<string, TranslationObject> Translations { get; set; }

        [JsonProperty("examples")]
        public Example[] Examples { get; set; } = [];

        [JsonProperty("semantic_category", NullValueHandling = NullValueHandling.Ignore)]
        public string SemanticCategory { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}