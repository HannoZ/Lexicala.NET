using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Test
{
#pragma warning disable 1591
    /// <summary>
    /// Represents the health-check response payload from the test endpoint.
    /// </summary>
    /// <remarks>
    /// Populated by <see cref="ILexicalaClient.TestAsync"/>.
    /// </remarks>
    public class TestResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
#pragma warning restore 1591
}

