using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class Sense
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("range_of_application", NullValueHandling = NullValueHandling.Ignore)]
        public string RangeOfApplication { get; set; }

        [JsonProperty("antonyms", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Antonyms { get; set; }

        [JsonProperty("synonyms", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Synonyms { get; set; } = { };

        [JsonProperty("translations")]
        public Dictionary<string, LanguageObject> Translations { get; set; }

        [JsonProperty("examples")] 
        public Example[] Examples { get; set; } = { };

        [JsonProperty("compositional_phrases")]
        public CompositionalPhrase[] CompositionalPhrases { get; set; } = { };
    
        [JsonProperty("semantic_subcategory", NullValueHandling = NullValueHandling.Ignore)]
        public string SemanticSubcategory { get; set; }

        [JsonProperty("geographical_usage", NullValueHandling = NullValueHandling.Ignore)]
        public string GeographicalUsage { get; set; }
    }
}