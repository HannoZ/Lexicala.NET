using System.Collections.Generic;

namespace Lexicala.NET.Parsing.Dto
{
    /// <summary>
    /// Represents a sense.
    /// </summary>
    public class Sense
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        public string Definition { get; set; }
        /// <summary>
        /// Gets the translations.
        /// </summary>
        public ICollection<Translation> Translations { get; } = new List<Translation>();
        /// <summary>
        /// Gets the examples.
        /// </summary>
        public ICollection<Example> Examples { get; } = new List<Example>();
        /// <summary>
        /// Gets the synonyms.
        /// </summary>
        public string[] Synonyms { get; } = { };
        /// <summary>
        /// Gets compositional phrases.
        /// </summary>
        public ICollection<CompositionalPhrase> CompositionalPhrases { get; } = new List<CompositionalPhrase>();

    }
}