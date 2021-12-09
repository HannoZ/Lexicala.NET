using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
                    return new[] { HeadwordObject.Headword };
                }
                else
                {
                    return new Headword[0];
                }
            }
        }

        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
