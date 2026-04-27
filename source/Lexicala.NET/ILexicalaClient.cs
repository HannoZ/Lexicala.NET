using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Languages;
using Lexicala.NET.Response.Search;
using Lexicala.NET.Response.Test;

namespace Lexicala.NET
{
    /// <summary>
    /// The Lexicala client contains all the methods that can be executed on the Lexicala API. 
    /// </summary>
    public interface ILexicalaClient
    {
        /// <summary>
        ///  Test that the API is up.
        /// </summary>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error or is not responding.</exception>
        Task<TestResponse> TestAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets information about languages available through the API.
        /// </summary>
        /// <remarks>
        ///By default, results are from KD's Global series. Data from the Password Series and from Random House Webster's College Dictionary are also available. The Global series includes 25 monolingual cores, which are added translation equivalents, producing multilingual versions. The Password series consists of an English core, translated to 46 languages. The Random House Webster's College Dictionary is an extensive monolingual English dictionary.
        /// </remarks>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<LanguagesResponse> LanguagesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Search for entries in the 'Global' source.
        /// </summary>
        /// <remarks>
        /// The search result consists of a JSON object containing partial lexical information on entries that match the search criteria. To obtain further, more in-depth, information for each entry, see <see cref="GetEntryAsync"/>.
        /// </remarks>
        /// <param name="searchText">Specify a headword</param>
        /// <param name="sourceLanguage">Specify which source language to look in (must be a 2-character language code).</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="System.ArgumentException">Thrown when searchText is null or empty, or when sourceLanguage is not a valid 2-character language code.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<SearchResponse> BasicSearchAsync(string searchText, string sourceLanguage, string etag = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Search for entries in the 'Global' source and return full entries.
        /// </summary>
        /// <remarks>
        /// The search result consists of full entry objects that match the search criteria.
        /// </remarks>
        /// <param name="searchText">Specify a headword</param>
        /// <param name="sourceLanguage">Specify which source language to look in (must be a 2-character language code).</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="ArgumentException">Thrown when searchText is null or empty, or when sourceLanguage is not a valid 2-character language code.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<IEnumerable<Entry>> SearchEntriesAsync(string searchText, string sourceLanguage, string etag = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Search for entries in RDF/JSON-LD format.
        /// </summary>
        /// <param name="searchText">Specify a headword</param>
        /// <param name="sourceLanguage">Specify which source language to look in (must be a 2-character language code).</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="ArgumentException">Thrown when searchText is null or empty, or when sourceLanguage is not a valid 2-character language code.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<string> SearchRdfAsync(string searchText, string sourceLanguage, string etag = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Search for entries in RDF/JSON-LD format using the parameters that are provided in the <paramref name="searchRequest"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when searchRequest is null.</exception>
        /// <exception cref="ArgumentException">Thrown when searchRequest properties are invalid (e.g., null/empty Language or SearchText, invalid language codes, or invalid parameter values).</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<string> AdvancedSearchRdfAsync(AdvancedSearchRequest searchRequest, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieve an entry in RDF/JSON-LD format by entry ID.
        /// </summary>
        /// <param name="entryId">The entry ID</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="ArgumentException">Thrown when entryId is null or empty.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error or the entry is not found.</exception>
        Task<string> GetRdfAsync(string entryId, string etag = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Search for entries using the parameters that are provided in the <paramref name="searchRequest"/>.
        /// </summary>
        /// <remarks>
        /// The search result consists of a JSON object containing partial lexical information on entries that match the search criteria. To obtain further, more in-depth information for each entry, see <see cref="GetEntryAsync"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when searchRequest is null.</exception>
        /// <exception cref="ArgumentException">Thrown when searchRequest properties are invalid (e.g., null/empty Language or SearchText, invalid language codes, or invalid parameter values).</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest searchRequest, CancellationToken cancellationToken = default);
        /// <summary>
        /// Search for entries using the parameters that are provided in the <paramref name="searchRequest"/> and return full entries.
        /// </summary>
        /// <remarks>
        /// The search result consists of full entry objects that match the search criteria.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when searchRequest is null.</exception>
        /// <exception cref="ArgumentException">Thrown when searchRequest properties are invalid (e.g., null/empty Language or SearchText, invalid language codes, or invalid parameter values).</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<IEnumerable<Entry>> AdvancedSearchEntriesAsync(AdvancedSearchRequest searchRequest, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieve a dictionary entry by entry ID. 
        /// </summary>
        /// <remarks>
        /// When searching by parameters (<see cref="BasicSearchAsync"/>/<see cref="AdvancedSearchAsync"/>), each entry result contains a unique entry ID, and each sense of an entry has its own unique sense ID. Using these IDs, it is possible to obtain more data – various syntactic and semantic information, compositional phrases, usage examples, translations and more – of a single entry (or sense). The entries collection groups together all entries from all different resources (Global, Password, Random House).
        /// </remarks>
        /// <param name="entryId">The entry ID</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="ArgumentException">Thrown when entryId is null or empty.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error or the entry is not found.</exception>
        Task<Entry> GetEntryAsync(string entryId, string etag = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieve a sense by it's ID. 
        /// </summary>
        /// <param name="senseId">The sense ID</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="ArgumentException">Thrown when senseId is null or empty.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error or the sense is not found.</exception>
        Task<Response.Entries.Sense> GetSenseAsync(string senseId, string etag = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Search for entries by performing a free-text search in definitions.
        /// </summary>
        /// <remarks>
        /// Performs a full-text search in definitions across 20 supported languages (ar, br, cs, da, de, el, en, es, fr, he, hi, it, ja, ko, nl, no, pl, pt, ru, sv, th, tr).
        /// </remarks>
        /// <param name="searchText">The text to search for in definitions</param>
        /// <param name="language">Optional. Filters results to match entries in the specified language (must be a 2-character language code if provided). The search text itself can be in any language.</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="ArgumentException">Thrown when searchText is null or empty, or when language is provided but is not a valid 2-character language code.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<SearchResponse> SearchDefinitionsAsync(string searchText, string language = null, string etag = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get a randomly selected entry for word discovery.
        /// </summary>
        /// <remarks>
        /// Returns a random entry from the available resources, useful for discovering words across supported languages.
        /// </remarks>
        /// <param name="source">Specify which resource to look in (global, password, multigloss, random). Default is global.</param>
        /// <param name="language">Optional. Specify which source language to look in (must be a 2-character language code if provided); if not specified, the language is chosen randomly.</param>
        /// <param name="etag">Optional.</param>
        /// <exception cref="ArgumentException">Thrown when source is invalid, or when language is provided but is not a valid 2-character language code.</exception>
        /// <exception cref="LexicalaApiException">Thrown when the API returns an error.</exception>
        Task<SearchResponse> FlukySearchAsync(string source = "global", string language = null, string etag = null, CancellationToken cancellationToken = default);
    }
}