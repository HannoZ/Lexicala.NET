using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Languages
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class LanguagesResponse
    {
        [JsonPropertyName("language_names")]
        public Dictionary<string, string> LanguageNames { get; set; }

        [JsonPropertyName("resources")]
        public Resources Resources { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

