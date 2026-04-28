using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Languages
{
    /// <summary>
    /// Represents source and target languages available for a resource.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Gets or sets the available source language codes.
        /// </summary>
        [JsonPropertyName("source_languages")]
        public string[] SourceLanguages { get; set; }

        /// <summary>
        /// Gets or sets the available target language codes.
        /// </summary>
        [JsonPropertyName("target_languages")]
        public string[] TargetLanguages { get; set; }
    }
}
