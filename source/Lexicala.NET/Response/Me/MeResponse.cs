using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MeResponse
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("permissions")]
        public Permissions Permissions { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

