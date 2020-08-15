using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Me
{
    public class Activation
    {
        [JsonProperty("activated")]
        public bool Activated { get; set; }

        [JsonProperty("agreed_terms_of_use")]
        public bool AgreedTermsOfUse { get; set; }
    }
}