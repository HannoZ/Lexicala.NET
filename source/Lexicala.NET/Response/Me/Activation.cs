using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Activation
    {
        [JsonPropertyName("activated")]
        public bool Activated { get; set; }

        [JsonPropertyName("agreed_terms_of_use")]
        public bool AgreedTermsOfUse { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
