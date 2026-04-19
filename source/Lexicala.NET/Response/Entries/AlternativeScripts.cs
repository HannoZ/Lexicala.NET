using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class AlternativeScripts
    {
        [JsonPropertyName("type")]
        public TypeEnum Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    public enum TypeEnum { Romaji };
}
