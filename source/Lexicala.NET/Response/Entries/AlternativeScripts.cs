using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents an alternative script rendering for a lexical form.
    /// </summary>
    /// <remarks>
    /// The Lexicala structure describes alternative scripts as script-name/text pairs.
    /// This model captures a normalized typed representation used by this SDK.
    /// </remarks>
    public class AlternativeScripts
    {
        /// <summary>
        /// Gets or sets the alternative script type.
        /// </summary>
        [JsonPropertyName("type")]
        public TypeEnum Type { get; set; }

        /// <summary>
        /// Gets or sets the text value in the alternative script.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    /// <summary>
    /// Supported alternative script types represented by this SDK.
    /// </summary>
    public enum TypeEnum
    {
        /// <summary>
        /// Romanized Japanese representation.
        /// </summary>
        Romaji
    }
}
