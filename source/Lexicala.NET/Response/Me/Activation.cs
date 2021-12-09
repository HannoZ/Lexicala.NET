using Newtonsoft.Json;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Activation
    {
        [JsonProperty("activated")]
        public bool Activated { get; set; }

        [JsonProperty("agreed_terms_of_use")]
        public bool AgreedTermsOfUse { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}