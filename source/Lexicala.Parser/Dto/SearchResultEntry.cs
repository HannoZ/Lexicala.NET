using System.Collections.Generic;
using System.Linq;

namespace Lexicala.NET.Parser.Dto
{
    public class SearchResultEntry
    {
        public string ETag { get; set; }
        public string Id { get; set; }
        public string Text { get; set; }
        public string Gender { get; set; }
        public string Pos { get; set; } 
        public string Pronunciation { get; set; }
        public string SubCategory { get; set; }
        public ICollection<Sense> Senses { get; set; } = new List<Sense>();
        public ICollection<string> Stems { get; set; } = new List<string>();
        public ICollection<Inflection> Inflections { get; set; } = new List<Inflection>();
        public string Summary(string languageCode) => string.Join(", ", Senses.SelectMany(s => s.Translations.Where(t => t.Language == languageCode).Select(t => t.Text)).Distinct());
    }
}