using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
    public class Language
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("gender")]
        public string Gender { get; set; }
       
        [JsonProperty("inflections", NullValueHandling = NullValueHandling.Ignore)]
        public Inflection[] Inflections { get; set; }

        [JsonProperty("alternative_scripts", NullValueHandling = NullValueHandling.Ignore)]
        public AlternativeScripts AlternativeScripts { get; set; }
    }
}