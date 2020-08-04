using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class SenseTranslations
    {
        [JsonProperty("br")]
        public CommonLanguageObject Br { get; set; }
        [JsonProperty("dk")]
        public CommonLanguageObject Dk { get; set; }
        [JsonProperty("en")]
        public CommonLanguageObject En { get; set; }
        [JsonProperty("fr")]
        public CommonLanguageObject Fr { get; set; }
        [JsonProperty("nl")]
        public CommonLanguageObject Nl { get; set; }
        [JsonProperty("no")]
        public CommonLanguageObject No { get; set; }
        [JsonProperty("sv")]
        public CommonLanguageObject Sv { get; set; }
        [JsonProperty("ja")]
        public Japanese Ja { get; set; }
    }
}