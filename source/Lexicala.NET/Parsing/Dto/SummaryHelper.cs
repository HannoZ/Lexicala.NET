using System.Collections.Generic;
using System.Linq;

namespace Lexicala.NET.Parsing.Dto
{
    public class SummaryHelper
    {
        public static string CreateSummary(string searchText, ICollection<SearchResultEntry> results, string languageCode)
        {
            string summary = string.Empty;
            foreach (var searchResult in results.Where(r=> !string.IsNullOrWhiteSpace(r.Summary(languageCode))))
            {
                if (searchResult.Text == searchText)
                {
                    summary += searchResult.Summary(languageCode) + " | ";
                }
                else
                {
                    summary += $"{searchResult.Text}: {searchResult.Summary(languageCode)} | ";
                }
            }

            return summary.TrimEnd(' ', '|');
        }
    }
}