using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Usage
    {
        [JsonPropertyName("today")]
        public Today Today { get; set; }

        [JsonPropertyName("lifetime")]
        public long Lifetime { get; set; }

        [JsonPropertyName("history")]
        public Dictionary<string, long> History { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
