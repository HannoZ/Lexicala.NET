using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Inflection
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("tense")]
        public string Tense { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}