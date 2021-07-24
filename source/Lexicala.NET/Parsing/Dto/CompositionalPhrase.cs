using System.Collections.Generic;

namespace Lexicala.NET.Parsing.Dto
{
    public class CompositionalPhrase
    {
        public string Text { get; set; }
        public string Definition { get; set; }
        public ICollection<Translation> Translations { get; set; } = new List<Translation>();
        public ICollection<Example> Examples { get; set; } = new List<Example>();

    }
}