using System;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Today
    {
        [JsonPropertyName("count")]
        public long Count { get; set; }

        [JsonPropertyName("ends_at")]
        public DateTimeOffset EndsAt { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
