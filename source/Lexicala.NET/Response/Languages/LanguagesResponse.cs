using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Languages
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class LanguagesResponse
    {
        [JsonProperty("language_names")]
        public Dictionary<string, string> LanguageNames { get; set; }

        [JsonProperty("resources")]
        public Resources Resources { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
