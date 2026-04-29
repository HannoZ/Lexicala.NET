using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents an inflected form for a headword or translation.
    /// </summary>
    public class Inflection
    {
        /// <summary>
        /// Gets or sets the inflected surface text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets grammatical number for this inflection, when available.
        /// </summary>
        [JsonPropertyName("number")]
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets grammatical tense for this inflection, when available.
        /// </summary>
        [JsonPropertyName("tense")]
        public string Tense { get; set; }
    }
}
