using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
    public class AlternativeScripts
    {
        [JsonProperty("romaji")]
        public string Romaji { get; set; }
    }
}