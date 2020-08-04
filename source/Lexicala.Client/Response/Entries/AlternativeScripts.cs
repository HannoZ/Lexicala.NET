using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class AlternativeScripts
    {
        [JsonProperty("romaji")]
        public string Romaji { get; set; }
    }
}