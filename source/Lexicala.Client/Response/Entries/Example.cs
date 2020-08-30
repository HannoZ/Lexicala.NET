using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class Example
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("translations", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, LanguageObject> Translations { get; set; }
    }
}