using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a lexical sense within an entry or from direct sense retrieval.
    /// </summary>
    /// <remarks>
    /// Populated as part of <see cref="Entry.Senses"/> and by
    /// <see cref="ILexicalaClient.GetSenseAsync"/> for direct sense fetches.
    /// </remarks>
    public class Sense
    {
        /// <summary>
        /// Gets or sets the unique sense identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the sense definition text.
        /// </summary>
        [JsonPropertyName("definition")]
        public string Definition { get; set; }

        /// <summary>
        /// Gets or sets range-of-application information for this sense.
        /// </summary>
        [JsonPropertyName("range_of_application")]
        public string RangeOfApplication { get; set; }

        /// <summary>
        /// Gets or sets antonyms associated with this sense.
        /// </summary>
        [JsonPropertyName("antonyms")]
        public string[] Antonyms { get; set; } = [];

        /// <summary>
        /// Gets or sets synonyms associated with this sense.
        /// </summary>
        [JsonPropertyName("synonyms")]
        public string[] Synonyms { get; set; } = [];

        /// <summary>
        /// Gets or sets translations keyed by 2-letter language code.
        /// </summary>
        [JsonPropertyName("translations")]
        public Dictionary<string, TranslationObject> Translations { get; set; } = [];

        /// <summary>
        /// Gets or sets usage examples for this sense.
        /// </summary>
        [JsonPropertyName("examples")] 
        public Example[] Examples { get; set; } = [];

        /// <summary>
        /// Gets or sets compositional phrases linked to this sense.
        /// </summary>
        [JsonPropertyName("compositional_phrases")]
        public CompositionalPhrase[] CompositionalPhrases { get; set; } = [];

        /// <summary>
        /// Gets or sets semantic subcategory information.
        /// </summary>
        [JsonPropertyName("semantic_subcategory")]
        public string SemanticSubcategory { get; set; }

        /// <summary>
        /// Gets or sets geographical usage information.
        /// </summary>
        [JsonPropertyName("geographical_usage")]
        public string GeographicalUsage { get; set; }

        /// <summary>
        /// Gets or sets response header metadata for direct sense retrieval calls.
        /// </summary>
        /// <remarks>
        /// This value is typically populated for top-level sense responses and may be null for
        /// nested senses that are part of an entry payload.
        /// </remarks>
        public ResponseMetadata Metadata { get; set; }
    }
}
