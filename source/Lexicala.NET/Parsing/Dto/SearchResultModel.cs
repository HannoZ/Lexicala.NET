using System.Collections.Generic;

namespace Lexicala.NET.Parsing.Dto
{
    public class SearchResultModel
    {
        public string ETag { get; set; }
        public string SearchText { get; set; }
        public ICollection<SearchResultEntry> Results { get; set; } = new List<SearchResultEntry>();
        public string Summary(string languageCode) => SummaryHelper.CreateSummary(SearchText, Results, languageCode);
    }
}
