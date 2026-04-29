using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Languages
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Resources
    {
        [JsonPropertyName("global")]
        public Resource Global { get; set; }

        [JsonPropertyName("password")]
        public Resource Password { get; set; }

        [JsonPropertyName("random")]
        public Resource Random { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
