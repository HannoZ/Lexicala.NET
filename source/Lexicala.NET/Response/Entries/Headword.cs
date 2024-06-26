﻿using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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