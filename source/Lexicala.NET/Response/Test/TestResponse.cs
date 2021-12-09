using Newtonsoft.Json;

namespace Lexicala.NET.Response.Test
{
#pragma warning disable 1591
    public class TestResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
#pragma warning restore 1591
}
