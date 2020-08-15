using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Me
{
    public class Today
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("ends_at")]
        public DateTimeOffset EndsAt { get; set; }
    }
}