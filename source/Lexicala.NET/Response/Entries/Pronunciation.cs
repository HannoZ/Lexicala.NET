using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a pronunciation value for a lexical item.
    /// </summary>
    public class Pronunciation
    {
        /// <summary>
        /// Gets or sets the pronunciation text.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
