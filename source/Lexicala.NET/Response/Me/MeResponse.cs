using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Represents account/profile information returned by the "me" endpoint.
    /// </summary>
    /// <remarks>
    /// This model maps identity, permission, and usage details for the authenticated API key.
    /// </remarks>
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

