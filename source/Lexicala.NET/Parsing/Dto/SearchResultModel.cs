using Lexicala.NET.Response;
using System.Collections.Generic;

namespace Lexicala.NET.Parsing.Dto
{
    /// <summary>
    /// The result of a search query. 
    /// </summary>
    public class SearchResultModel
    {
        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        public string SearchText { get; set; }
        /// <summary>
        /// Gets the results.
        /// </summary>
        public ICollection<SearchResultEntry> Results { get; internal set; } = new List<SearchResultEntry>();
        /// <summary>
        /// Gets or sets the total number search results.
        /// </summary>
        public int TotalResults { get; set; }
        /// <summary>
        /// Gets or sets the response metadata (ETag, rate limits)
        /// </summary>
        public ResponseMetadata Metadata { get; set; }
        /// <summary>
        /// Gets the summary for this search.
        /// </summary>
        /// <param name="languageCode">The desired language for the summary.</param>
        /// <returns>The summary. Empty string if there is no result or translations for the desired language.</returns>
        public string Summary(string languageCode) => SummaryHelper.CreateSummary(SearchText, Results, languageCode);
    }
}
