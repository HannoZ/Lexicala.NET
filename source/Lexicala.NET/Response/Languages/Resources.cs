using Newtonsoft.Json;

namespace Lexicala.NET.Response.Languages
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Resources
    {
        [JsonProperty("global")]
        public Resource Global { get; set; }

        [JsonProperty("password")]
        public Resource Password { get; set; }

        [JsonProperty("random")]
        public Resource Random { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}