using System.Collections.Generic;

namespace Lexicala.NET.Parsing.Dto
{
    /// <summary>
    /// Represents an example. Examples exist of a sentence and one or more translations.
    /// </summary>
    public class Example
    {
        /// <summary>
        /// Gets or sets the example sentence.
        /// </summary>
        public string Sentence { get; set; }
        /// <summary>
        /// Gets the translations.
        /// </summary>
        public ICollection<Translation> Translations { get; } = new List<Translation>();
    }
}