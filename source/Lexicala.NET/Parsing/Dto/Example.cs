using System.Collections.Generic;

namespace Lexicala.NET.Parsing.Dto
{
    public class Example
    {
        public string Sentence { get; set; }
        public ICollection<Translation> Translations { get; set; } = new List<Translation>();
    }
}