using System;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Permissions
    {
        [JsonPropertyName("activation")]
        public Activation Activation { get; set; }

        [JsonPropertyName("pro")]
        public bool Pro { get; set; }

        [JsonPropertyName("enterprise")]
        public bool Enterprise { get; set; }

        [JsonPropertyName("requests_per_day")]
        public long RequestsPerDay { get; set; }

        [JsonPropertyName("creation_date")]
        public DateTimeOffset CreationDate { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
