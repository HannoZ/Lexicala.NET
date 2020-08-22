namespace Lexicala.NET.Client.Request
{
    public class AdvancedSearchRequest
    {
        public string ETag { get; set; }
        public string Source { get; set; } = Sources.Global;
        public string SearchText { get; set; }
        public string Language { get; set; }

        public bool Analyzed { get; set; }
        public bool Monosemous { get; set; }
        public bool Morph { get; set; }
        public bool Polysemous { get; set; }

        public string Gender { get; set; }
        public string Number{ get; set; }
        public string Pos { get; set; }
        public int Page { get; set; } = 1;
        public int PageLength { get; set; } = 10;
        public int Sample { get; set; }
        public string Subcategorization { get; set; }
    }
}
