using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lexicala.NET.Client.Response.Languages
{
    public class LanguagesResponse
    {
        [JsonProperty("language_names")]
        public Dictionary<string, string> LanguageNames { get; set; }

        [JsonProperty("resources")]
        public Resources Resources { get; set; }
    }
}
