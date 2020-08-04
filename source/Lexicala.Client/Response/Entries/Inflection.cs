using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class Inflection
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}