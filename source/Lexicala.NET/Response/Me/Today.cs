using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Today
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("ends_at")]
        public DateTimeOffset EndsAt { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}