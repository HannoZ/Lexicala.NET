using System.Collections.Generic;

namespace Lexicala.NET.Parser.Dto
{
    public class Example
    {
        public string Sentence { get; set; }
        public ICollection<Translation> Translations { get; set; } = new List<Translation>();
    }
}