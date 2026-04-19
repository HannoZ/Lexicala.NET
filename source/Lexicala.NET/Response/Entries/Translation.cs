using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Translation
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        
        [JsonPropertyName("gender")]
        public string Gender { get; set; }
       
        [JsonPropertyName("inflections")]
        public Inflection[] Inflections { get; set; }

        [JsonPropertyName("alternative_scripts")]
        public AlternativeScripts [] AlternativeScripts { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
