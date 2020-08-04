using System.Collections.Generic;

namespace Lexicala.NET.Parser.Dto
{
    public class Sense
    {
        public string Id { get; set; }
        public string Definition { get; set; }
        public ICollection<Translation> Translations { get; set; } = new List<Translation>();
        public ICollection<Example> Examples { get; set; } = new List<Example>();
        public string[] Synonyms { get; set; } = { };
    }
}