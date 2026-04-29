using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents a dictionary entry returned by entry-oriented endpoints.
    /// </summary>
    /// <remarks>
    /// Populated by <see cref="ILexicalaClient.GetEntryAsync"/> and by search-entry style endpoints
    /// returning entry payloads.
    /// </remarks>
    public class Entry
    {
        /// <summary>
        /// Gets or sets the unique dictionary entry identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the source resource name (for example: global, password, multigloss, random).
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the 2-letter source language code.
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the version number of the source lexical resource.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets frequency information derived from corpus data.
        /// </summary>
        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }

        /// <summary>
        /// Gets or sets headword data in single-or-array wrapper format.
        /// </summary>
        [JsonPropertyName("headword")]
        public HeadwordObject HeadwordObject { get; set; }

        /// <summary>
        /// Gets or sets senses belonging to this entry.
        /// </summary>
        [JsonPropertyName("senses")] 
        public Sense[] Senses { get; set; } = [];

        /// <summary>
        /// Gets or sets related entry identifiers.
        /// </summary>
        [JsonPropertyName("related_entries")] 
        public string[] RelatedEntries { get; set; } = [];

        /// <summary>
        /// Gets normalized headword values as an array.
        /// </summary>
        /// <remarks>
        /// Lexicala may return <c>headword</c> as either one object or an array.
        /// This accessor normalizes both shapes to an array.
        /// </remarks>
        public Headword[] Headwords
        {
            get
            {
                if (HeadwordObject.HeadwordElementArray != null)
                {
                    return HeadwordObject.HeadwordElementArray;
                }
                else if (HeadwordObject.Headword != null)
                {
                    return [HeadwordObject.Headword];
                }
                else
                {
                    return [];
                }
            }
        }

        /// <summary>
        /// Gets or sets response header metadata (ETag and rate limits) associated with this entry.
        /// </summary>
        /// <remarks>
        /// The ETag can be reused in conditional requests with <c>If-None-Match</c> to optimize
        /// caching behavior.
        /// </remarks>
        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
    }
}

