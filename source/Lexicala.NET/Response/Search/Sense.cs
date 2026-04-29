using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
    /// <summary>
    /// Represents a sense summary in search results.
    /// </summary>
    public class Sense
    {
        /// <summary>
        /// Gets or sets the sense identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the sense definition text.
        /// </summary>
        [JsonPropertyName("definition")]
        public string Definition { get; set; }
    }
}
