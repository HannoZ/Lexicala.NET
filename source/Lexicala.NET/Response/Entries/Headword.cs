using System;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Headword
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("pronunciation")]
        public PronunciationObject PronunciationObject { get; set; }

        [JsonPropertyName("pos")]
        public Pos Pos { get; set; }

        [JsonPropertyName("homograph_number")]
        public int HomographNumber { get; set; }

        [JsonPropertyName("subcategorization")]
        public string Subcategorization { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("inflections")]
        public Inflection[] Inflections { get; set; }

        [JsonPropertyName("additional_inflections")]
        public string[] AdditionalInflections { get; set; } = [];

        public Pronunciation[] Pronunciations
        {
            get
            {
                if (PronunciationObject.PronunciationArray != null)
                {
                    return PronunciationObject.PronunciationArray;
                }
                else if (PronunciationObject.Pronunciation != null)
                {
                    return [PronunciationObject.Pronunciation];
                }
                else
                {
                    return [];
                }
            }
        }

        public string[] PartOfSpeeches => Pos.PartOfSpeechArray ?? [Pos.PartOfSpeech];
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
