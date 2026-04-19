using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Test
{
#pragma warning disable 1591
    public class TestResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
#pragma warning restore 1591
}

