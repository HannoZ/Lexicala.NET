using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a translation value for a target language.
    /// </summary>
    public class Translation
    {
        /// <summary>
        /// Gets or sets the translated text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets grammatical gender for this translation, when available.
        /// </summary>
        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets inflections associated with this translation.
        /// </summary>
        [JsonPropertyName("inflections")]
        public Inflection[] Inflections { get; set; }

        /// <summary>
        /// Gets or sets alternative script values for this translation.
        /// </summary>
        [JsonPropertyName("alternative_scripts")]
        public AlternativeScripts[] AlternativeScripts { get; set; }
    }
}
