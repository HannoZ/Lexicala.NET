using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class AlternativeScripts
    {
        [JsonProperty("romaji")]
        public string Romaji { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}