using System.Threading.Tasks;
using Lexicala.NET.Parser.Dto;

namespace Lexicala.NET.Parser
{
    public interface ILexicalaSearchParser
    {
        /// <summary>
        /// Executes a search request and subsequent calls to load the entry information of the search result.
        /// </summary>
        Task<SearchResultModel> SearchAsync(string searchTerm, string language);
    }
}