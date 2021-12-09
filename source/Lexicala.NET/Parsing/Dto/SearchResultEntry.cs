using System.Collections.Generic;
using System.Linq;

namespace Lexicala.NET.Parsing.Dto
{
    /// <summary>
    /// Represents a single search result entry. 
    /// </summary>
    public class SearchResultEntry
    {
        /// <summary>
        /// Gets or sets the ETag
        /// </summary>
        public string ETag { get; set; }
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Gets or sets the part of speech.
        /// </summary>
        public string Pos { get; set; } 
        /// <summary>
        /// Gets or sets the pronunciation.
        /// </summary>
        public string Pronunciation { get; set; }
        /// <summary>
        /// Gets or sets the sub category.
        /// </summary>
        public string SubCategory { get; set; }
        /// <summary>
        /// Gets the senses.
        /// </summary>
        public ICollection<Sense> Senses { get; } = new List<Sense>();
        /// <summary>
        /// Gets the stems.
        /// </summary>
        public ICollection<string> Stems { get; } = new List<string>();
        /// <summary>
        /// Gets the inflections.
        /// </summary>
        public ICollection<Inflection> Inflections { get; } = new List<Inflection>();
        /// <summary>
        /// Gets the summary for this entry.
        /// </summary>
        /// <param name="languageCode">The desired language for the summary.</param>
        /// <returns>The summary for the desired language, or empty string if there are no senses or available translations</returns>
        public string Summary(string languageCode) => string.Join(", ", Senses.SelectMany(s => s.Translations.Where(t => t.Language == languageCode).Select(t => t.Text)).Distinct());
    }
}