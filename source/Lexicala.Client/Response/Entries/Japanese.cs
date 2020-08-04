using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class Japanese
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("alternative_scripts")]
        public AlternativeScripts AlternativeScripts { get; set; }
    }
}