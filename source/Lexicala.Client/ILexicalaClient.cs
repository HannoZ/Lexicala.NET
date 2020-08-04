using System.Threading.Tasks;
using Lexicala.NET.Client.Request;
using Lexicala.NET.Client.Response.Entries;
using Lexicala.NET.Client.Response.Search;

namespace Lexicala.NET.Client
{
    public interface ILexicalaClient
    {
        Task<SearchResponse> BasicSearchAsync(string searchText, string language, string etag = null);
        Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest request);
        Task<Entry> GetEntryAsync(string entryId, string etag = null);
    }
}