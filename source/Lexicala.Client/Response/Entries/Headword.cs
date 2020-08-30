using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries
{
    public class Headword
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("pronunciation")]
        public PronunciationObject Pronunciation { get; set; }

        [JsonProperty("pos", NullValueHandling = NullValueHandling.Ignore)]
        public Pos Pos { get; set; }

        [JsonProperty("homograph_number")]
        public int HomographNumber { get; set; }

        [JsonProperty("subcategorization")]
        public string Subcategorization { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("inflections")]
        public Inflection[] Inflections { get; set; }

        [JsonProperty("additional_inflections")]
        public string[] AdditionalInflections { get; set; } = { };
    }
}