using Newtonsoft.Json;

namespace Lexicala.NET.Response.Me
{
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
}
