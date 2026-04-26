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
        /// <param name="searchTerm">The text to search for</param>
        /// <param name="sourceLanguage">The source language code (e.g., "en", "es", "de") - must be a valid 2-character language code</param>
        /// <param name="targetLanguages">Specifies the languages for which translations are returned (if any). When no specific languages are specified, all available translations are returned.</param>
        /// <exception cref="ArgumentException">Thrown when searchTerm is null or empty, or when sourceLanguage is not a valid 2-character language code present in the available source languages.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<SearchResultModel> SearchAsync(string searchTerm, string sourceLanguage, params string [] targetLanguages);
        /// <summary>
        /// Executes a search request and subsequent calls to load the entry information of the search result.
        /// </summary>
        /// <param name="searchRequest">The advanced search request containing search parameters</param>
        /// <param name="targetLanguages">Specifies the languages for which translations are returned (if any). When no specific languages are specified, all available translations are returned.</param>
        /// <exception cref="ArgumentNullException">Thrown when searchRequest is null.</exception>
        /// <exception cref="ArgumentException">Thrown when searchRequest properties are invalid (e.g., null/empty Language or SearchText, or Language is not a valid 2-character language code present in the available source languages).</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<SearchResultModel> SearchAsync(AdvancedSearchRequest searchRequest, params string[] targetLanguages);
        /// <summary>
        /// Gets the specified entry. 
        /// </summary>
        /// <param name="entryId">The entry ID (obtained from a search request)</param>
        /// <param name="targetLanguages">Specifies the languages for which translations are returned (if any). When no specific languages are specified, all available translations are returned.</param>
        /// <exception cref="ArgumentException">Thrown when entryId is null or empty.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error or the entry is not found.</exception>
        Task<SearchResultEntry> GetEntryAsync(string entryId, params string[] targetLanguages);
    }
}