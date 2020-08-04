using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class CommonLanguage
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("inflections", NullValueHandling = NullValueHandling.Ignore)]
        public Inflection[] Inflections { get; set; }
    }
}