using Newtonsoft.Json;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MeResponse
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; set; }

        [JsonProperty("usage")]
        public Usage Usage { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
