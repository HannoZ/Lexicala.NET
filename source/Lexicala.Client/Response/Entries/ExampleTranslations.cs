using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class ExampleTranslations
    {
        [JsonProperty("br")]
        public CommonLanguage Br { get; set; }
        [JsonProperty("dk")]
        public CommonLanguage Dk { get; set; }
        [JsonProperty("en")]
        public CommonLanguage En { get; set; }
        [JsonProperty("nl")]
        public CommonLanguage Nl { get; set; }
        [JsonProperty("no")]
        public CommonLanguage No { get; set; }
        [JsonProperty("sv")]
        public CommonLanguage Sv { get; set; }
        [JsonProperty("ja")]
        public Japanese Ja { get; set; }
    }
}