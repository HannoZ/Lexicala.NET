using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a usage example for a sense or compositional phrase.
    /// </summary>
    public class Example
    {
        /// <summary>
        /// Gets or sets the example sentence text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets example translations keyed by 2-letter language code.
        /// </summary>
        [JsonPropertyName("translations")]
        public Dictionary<string, TranslationObject> Translations { get; set; }
    }
}
