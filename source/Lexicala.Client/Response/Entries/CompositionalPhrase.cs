using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class CompositionalPhrase
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("semantic_subcategory", NullValueHandling = NullValueHandling.Ignore)]
        public string SemanticSubcategory { get; set; }

        [JsonProperty("translations")]
        public SenseTranslations Translations { get; set; }

        [JsonProperty("examples")]
        public Example[] Examples { get; set; }

        [JsonProperty("semantic_category", NullValueHandling = NullValueHandling.Ignore)]
        public string SemanticCategory { get; set; }
    }
}