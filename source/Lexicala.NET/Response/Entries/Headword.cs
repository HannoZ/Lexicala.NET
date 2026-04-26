using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries
{
    /// <summary>
    /// Represents detailed headword information for an entry.
    /// </summary>
    /// <remarks>
    /// Lexicala headword data includes orthography, part-of-speech, pronunciation,
    /// and morphological details such as inflections.
    /// </remarks>
    public class Headword
    {
        /// <summary>
        /// Gets or sets the headword surface text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets pronunciation data in single-or-array wrapper format.
        /// </summary>
        [JsonPropertyName("pronunciation")]
        public PronunciationObject PronunciationObject { get; set; }

        /// <summary>
        /// Gets or sets part-of-speech data in single-or-array wrapper format.
        /// </summary>
        [JsonPropertyName("pos")]
        public Pos Pos { get; set; }

        /// <summary>
        /// Gets or sets the homograph number for this headword.
        /// </summary>
        [JsonPropertyName("homograph_number")]
        public int HomographNumber { get; set; }

        /// <summary>
        /// Gets or sets grammatical subcategorization information.
        /// </summary>
        [JsonPropertyName("subcategorization")]
        public string Subcategorization { get; set; }

        /// <summary>
        /// Gets or sets grammatical gender, when available.
        /// </summary>
        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets inflection forms associated with this headword.
        /// </summary>
        [JsonPropertyName("inflections")]
        public Inflection[] Inflections { get; set; }

        /// <summary>
        /// Gets or sets additional inflectional forms as plain text values.
        /// </summary>
        [JsonPropertyName("additional_inflections")]
        public string[] AdditionalInflections { get; set; } = [];

        /// <summary>
        /// Gets normalized pronunciation values as an array.
        /// </summary>
        /// <remarks>
        /// Lexicala may return pronunciation as either a single object or an array.
        /// This accessor normalizes both shapes to an array.
        /// </remarks>
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

        /// <summary>
        /// Gets normalized part-of-speech values as an array.
        /// </summary>
        public string[] PartOfSpeeches => Pos.PartOfSpeechArray ?? [Pos.PartOfSpeech];
    }
}
