using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
    public class Example
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("translations", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, TranslationObject> Translations { get; set; }
    }
}