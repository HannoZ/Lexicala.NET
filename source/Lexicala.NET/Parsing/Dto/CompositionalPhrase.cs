using System.Collections.Generic;

namespace Lexicala.NET.Parsing.Dto
{
    /// <summary>
    /// Represents a compositional phrase.
    /// </summary>
    /// <remarks>
    /// A compositional phrase can contain a definition with translations and examples, or a collection of one or more senses.
    /// </remarks>
    public class CompositionalPhrase
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// <i>Optional.</i>Gets or sets the definition.
        /// </summary>
        public string Definition { get; set; }
        /// <summary>
        /// <i>Optional.</i>Gets the translations.
        /// </summary>
        public ICollection<Translation> Translations { get; } = new List<Translation>();
        /// <summary>
        /// <i>Optional.</i> Gets the examples.
        /// </summary>
        public ICollection<Example> Examples { get; } = new List<Example>();
        /// <summary>
        /// <i>Optional.</i> Gets the senses.
        /// </summary>
        public ICollection<Sense> Senses { get; } = new List<Sense>();
    }
}