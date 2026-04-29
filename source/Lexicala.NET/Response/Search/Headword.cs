using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
    /// <summary>
    /// Represents a headword item in a search response.
    /// </summary>
    public class Headword
    {
        /// <summary>
        /// Gets or sets the headword text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the part of speech.
        /// </summary>
        [JsonPropertyName("pos")]
        public string Pos { get; set; }
    }
}
