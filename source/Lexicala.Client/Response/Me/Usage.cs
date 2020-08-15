using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Me
{
    public class Usage
    {
        [JsonProperty("today")]
        public Today Today { get; set; }

        [JsonProperty("lifetime")]
        public long Lifetime { get; set; }

        [JsonProperty("history")]
        public Dictionary<string, long> History { get; set; }
    }
}