using System;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Entry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }

        [JsonPropertyName("headword")]
        public HeadwordObject HeadwordObject { get; set; }

        [JsonPropertyName("senses")] 
        public Sense[] Senses { get; set; } = [];

        [JsonPropertyName("related_entries")] 
        public string[] RelatedEntries { get; set; } = [];

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

        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}

