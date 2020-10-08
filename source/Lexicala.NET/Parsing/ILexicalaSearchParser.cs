using System.Threading.Tasks;
using Lexicala.NET.Parsing.Dto;

namespace Lexicala.NET.Parsing
{
    public interface ILexicalaSearchParser
    {
        /// <summary>
        /// Executes a search request and subsequent calls to load the entry information of the search result.
        /// </summary>
        Task<SearchResultModel> SearchAsync(string searchTerm, string sourceLanguage, params string [] targetLanguages);

        Task<SearchResultEntry> GetEntryAsync(string entryId, params string[] targetLanguages);
    }
}