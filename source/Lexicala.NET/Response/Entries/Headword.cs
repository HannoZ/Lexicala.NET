﻿using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
    public class Headword
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("pronunciation")]
        public PronunciationObject PronunciationObject { get; set; }

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

        public Pronunciation[] Pronunciations => PronunciationObject.PronunciationArray ?? new[] {PronunciationObject.Pronunciation};
        public string[] PartOfSpeeches => Pos.PartOfSpeechArray ?? new[] {Pos.PartOfSpeech};
    }
}