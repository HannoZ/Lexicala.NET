using System.Threading.Tasks;
using Lexicala.NET.Client.Request;
using Lexicala.NET.Client.Response.Entries;
using Lexicala.NET.Client.Response.Languages;
using Lexicala.NET.Client.Response.Me;
using Lexicala.NET.Client.Response.Search;
using Lexicala.NET.Client.Response.Test;

namespace Lexicala.NET.Client
{
    public interface ILexicalaClient
    {
        /// <summary>
        ///  Test that the API is up.
        /// </summary>
        Task<TestResponse> TestAsync();
        /// <summary>
        /// View your user account settings.
        /// </summary>
        /// <remarks>
        /// This includes the personal details such as your name and the email you have provided upon registration, your request cap, and the number of requests used in the last 24 hours.
        /// </remarks>
        Task<MeResponse> MeAsync();
        /// <summary>
        /// Gets information about languages available through the API.
        /// </summary>
        /// <remarks>
        ///By default, results are from KD's Global series. Data from the Password Series and from Random House Webster's College Dictionary are also available. The Global series includes 24 monolingual cores, which are added translation equivalents, producing multilingual versions. The Password series consists of an English core, translated to 46 languages. The Random House Webster's College Dictionary is an extensive monolingual English dictionary.
        /// </remarks>
        Task<LanguagesResponse> LanguagesAsync();
        /// <summary>
        /// Search for entries in the 'Global' source.
        /// </summary>
        /// <remarks>
        /// The search result consists of a JSON object containing partial lexical information on entries that match the search criteria. To obtain further, more in-depth information for each entry, see <see cref="GetEntryAsync"/>.
        /// </remarks>
        /// <param name="searchText">Specify a headword</param>
        /// <param name="sourceLanguage">Specify which source language to look in.</param>
        /// <param name="etag">Optional.</param>
        Task<SearchResponse> BasicSearchAsync(string searchText, string sourceLanguage, string etag = null);
        /// <summary>
        /// Search for entries using the parameters that are provided in the <paramref name="searchRequest"/>.
        /// </summary>
        /// <remarks>
        /// The search result consists of a JSON object containing partial lexical information on entries that match the search criteria. To obtain further, more in-depth information for each entry, see <see cref="GetEntryAsync"/>.
        /// </remarks>
        Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest searchRequest);
        /// <summary>
        /// Retrieve a dictionary entry by entry ID. 
        /// </summary>
        /// <remarks>
        /// When searching by parameters (<see cref="BasicSearchAsync"/>/<see cref="AdvancedSearchAsync"/>), each entry result contains a unique entry ID, and each sense of an entry has its own unique sense ID. Using these IDs, it is possible to obtain more data – various syntactic and semantic information, compositional phrases, usage examples, translations and more – of a single entry (or sense). The entries collection groups together all entries from all different resources (Global, Password, Random House).
        /// </remarks>
        /// <param name="entryId">The entry ID</param>
        /// <param name="etag">Optional.</param>
        /// <returns></returns>
        Task<Entry> GetEntryAsync(string entryId, string etag = null);
    }
}