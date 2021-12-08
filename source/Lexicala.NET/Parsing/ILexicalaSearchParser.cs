using System.Threading.Tasks;
using Lexicala.NET.Parsing.Dto;
using Lexicala.NET.Request;

namespace Lexicala.NET.Parsing
{
    /// <summary>
    /// Contains helper methods to interact with the Lexicala api.
    /// </summary>
    public interface ILexicalaSearchParser
    {
        /// <summary>
        /// Executes a search request and subsequent calls to load the entry information of the search result.
        /// </summary>
        Task<SearchResultModel> SearchAsync(string searchTerm, string sourceLanguage, params string [] targetLanguages);
        /// <summary>
        /// Executes a search request and subsequent calls to load the entry information of the search result.
        /// </summary>
        Task<SearchResultModel> SearchAsync(AdvancedSearchRequest searchRequest, params string[] targetLanguages);
        /// <summary>
        /// Gets the specified entry. 
        /// </summary>
        /// <param name="entryId">The entry ID (obtained from a search request)</param>
        /// <param name="targetLanguages">Specifies the languages for which translations are returned (if any). When no specific languages are specified, all available translations are returned.</param>
        /// <returns></returns>
        Task<SearchResultEntry> GetEntryAsync(string entryId, params string[] targetLanguages);
    }
}