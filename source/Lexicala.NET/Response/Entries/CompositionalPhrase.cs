using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a compositional phrase attached to a lexical sense.
    /// </summary>
    /// <remarks>
    /// In Lexicala responses, compositional phrases can contain definition text,
    /// optional semantic labels, examples, translations, and nested senses.
    /// </remarks>
    public class CompositionalPhrase
    {
        /// <summary>
        /// Gets or sets the compositional phrase text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the phrase definition.
        /// </summary>
        [JsonPropertyName("definition")]
        public string Definition { get; set; }

        /// <summary>
        /// Gets or sets the semantic subcategory label for the phrase.
        /// </summary>
        [JsonPropertyName("semantic_subcategory")]
        public string SemanticSubcategory { get; set; }

        /// <summary>
        /// Gets or sets nested senses associated with this compositional phrase.
        /// </summary>
        [JsonPropertyName("senses")]
        public Sense[] Senses { get; set; } = [];

        /// <summary>
        /// Gets or sets phrase translations keyed by 2-letter language code.
        /// </summary>
        [JsonPropertyName("translations")]
        public Dictionary<string, TranslationObject> Translations { get; set; }

        /// <summary>
        /// Gets or sets usage examples for this compositional phrase.
        /// </summary>
        [JsonPropertyName("examples")]
        public Example[] Examples { get; set; } = [];

        /// <summary>
        /// Gets or sets the semantic category label for the phrase.
        /// </summary>
        [JsonPropertyName("semantic_category")]
        public string SemanticCategory { get; set; }
    }
}
