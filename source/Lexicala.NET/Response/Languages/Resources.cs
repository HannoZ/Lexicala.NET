using Newtonsoft.Json;

namespace Lexicala.NET.Response.Languages
{
    public class Resources
    {
        [JsonProperty("global")]
        public Resource Global { get; set; }

        [JsonProperty("password")]
        public Resource Password { get; set; }

        [JsonProperty("random")]
        public Resource Random { get; set; }
    }
}