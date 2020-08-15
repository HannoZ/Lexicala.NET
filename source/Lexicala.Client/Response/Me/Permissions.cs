using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Me
{
    public class Permissions
    {
        [JsonProperty("activation")]
        public Activation Activation { get; set; }

        [JsonProperty("pro")]
        public bool Pro { get; set; }

        [JsonProperty("enterprise")]
        public bool Enterprise { get; set; }

        [JsonProperty("requests_per_day")]
        public long RequestsPerDay { get; set; }

        [JsonProperty("creation_date")]
        public DateTimeOffset CreationDate { get; set; }
    }
}