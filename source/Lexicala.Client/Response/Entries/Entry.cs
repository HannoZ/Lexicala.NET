using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class Entry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("headword")]
        public HeadwordObject HeadwordObject { get; set; }

        [JsonProperty("senses")] 
        public Sense[] Senses { get; set; } = { };

        [JsonProperty("related_entries")] 
        public string[] RelatedEntries { get; set; } = { };

        public Headword[] Headwords => HeadwordObject.HeadwordElementArray ?? new[] { HeadwordObject.Headword };

        public ResponseMetadata Metadata { get; set; }
    }
}
