using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Translation
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("gender")]
        public string Gender { get; set; }
       
        [JsonProperty("inflections", NullValueHandling = NullValueHandling.Ignore)]
        public Inflection[] Inflections { get; set; }

        [JsonProperty("alternative_scripts", NullValueHandling = NullValueHandling.Ignore)]
        public AlternativeScripts [] AlternativeScripts { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}