using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Test
{
    public class TestResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
