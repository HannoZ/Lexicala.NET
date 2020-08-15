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
        Task<TestResponse> TestAsync();
        Task<MeResponse> MeAsync();
        Task<LanguagesResponse> LanguagesAsync();
        Task<SearchResponse> BasicSearchAsync(string searchText, string sourceLanguage, string etag = null);
        Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest request);
        Task<Entry> GetEntryAsync(string entryId, string etag = null);
    }
}