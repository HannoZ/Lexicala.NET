using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
    public class Pronunciation
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
    
}