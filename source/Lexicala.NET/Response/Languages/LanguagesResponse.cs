using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Languages
{
    public class LanguagesResponse
    {
        [JsonProperty("language_names")]
        public Dictionary<string, string> LanguageNames { get; set; }

        [JsonProperty("resources")]
        public Resources Resources { get; set; }
    }
}
