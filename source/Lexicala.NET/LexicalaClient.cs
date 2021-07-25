using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Entries.JsonConverters;
using Lexicala.NET.Response.Languages;
using Lexicala.NET.Response.Me;
using Lexicala.NET.Response.Search;
using Lexicala.NET.Response.Test;
using Newtonsoft.Json;

namespace Lexicala.NET
{
    public class LexicalaClient : ILexicalaClient
    {
        private readonly HttpClient _httpClient;

        private const string Test = "/test";
        private const string Me = "/users/me";
        private const string Search = "/search";
        private const string Entries = "/entries";
        private const string Languages = "/languages";

        public LexicalaClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TestResponse> TestAsync()
        {
            var response = await _httpClient.GetStringAsync(Test);
            return JsonConvert.DeserializeObject<TestResponse>(response);
        }

        public async Task<MeResponse> MeAsync()
        {
            var response = await _httpClient.GetStringAsync(Me);
            return JsonConvert.DeserializeObject<MeResponse>(response);
        }

        public async Task<LanguagesResponse> LanguagesAsync()
        {
            var response = await _httpClient.GetStringAsync(Languages);
            return JsonConvert.DeserializeObject<LanguagesResponse>(response);
        }

        public Task<SearchResponse> BasicSearchAsync(string searchText, string sourceLanguage, string etag = null)
        {
            if (sourceLanguage.Length != 2)
            {
                throw new ArgumentException($"Invalid language code provided ({sourceLanguage}), a valid language code is two characters", nameof(sourceLanguage));
            }

            if (string.IsNullOrEmpty(searchText))
            {
                throw new ArgumentException("SearchText cannot be empty", nameof(searchText));
            }

            return ExecuteSearch($"{Search}?language={sourceLanguage}&text={searchText}", etag);
        }

        public Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest searchRequest)
        {
            if (searchRequest.Language?.Length != 2)
            {
                throw new ArgumentException($"Invalid language code provided ({searchRequest.Language}), a valid language code is two characters");
            }

            if (string.IsNullOrEmpty(searchRequest.SearchText))
            {
                throw new ArgumentException("SearchText cannot be empty");
            }

            // build the querystring based on provided search request params
            StringBuilder queryStringBuilder = new StringBuilder($"{Search}?language={searchRequest.Language}&text={searchRequest.SearchText}");
            queryStringBuilder.Append("&source=" + searchRequest.Source);
            
            if (searchRequest.Analyzed)
            {
                queryStringBuilder.Append("&analyzed=true");
            }
            if (searchRequest.Monosemous)
            {
                queryStringBuilder.Append("&monosemous=true");
            }
            if (searchRequest.Polysemous)
            {
                queryStringBuilder.Append("&polysemous=true");
            }
            if (searchRequest.Morph)
            {
                queryStringBuilder.Append("&morph=true");
            }
            if (!string.IsNullOrEmpty(searchRequest.Pos))
            {
                queryStringBuilder.Append("&pos=" + searchRequest.Pos);
            }
            if (!string.IsNullOrEmpty(searchRequest.Number))
            {
                queryStringBuilder.Append("&number=" + searchRequest.Number);
            }
            if (!string.IsNullOrEmpty(searchRequest.Gender))
            {
                queryStringBuilder.Append("&gender=" + searchRequest.Gender);
            }
            if (!string.IsNullOrEmpty(searchRequest.Subcategorization))
            {
                queryStringBuilder.Append("&subcategorization=" + searchRequest.Subcategorization);
            }

            // pagination - only append if values are other than default values
            if (searchRequest.Page > 1)
            {
                queryStringBuilder.Append("&page=" + searchRequest.Page);
            }
            if (searchRequest.PageLength != 10 && searchRequest.PageLength > 0 && searchRequest.PageLength <= 30)
            {
                queryStringBuilder.Append("&page-length=" + searchRequest.PageLength);
            }
            if (searchRequest.Sample > 0)
            {
                queryStringBuilder.Append("&sample=" + searchRequest.Sample);
            }

            return ExecuteSearch(queryStringBuilder.ToString(), searchRequest.ETag);
        }

        private async Task<SearchResponse> ExecuteSearch(string querystring, string etag)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, querystring);

            if (etag != null)
            {
                httpRequest.Headers.Add("If-None-Match", etag);
            }

            using var response = await _httpClient.SendAsync(httpRequest);

            // until we have a better error-handling mechanism we just let the code throw built-in exception if request was not successful
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            var responseObject = JsonConvert.DeserializeObject<SearchResponse>(result, SearchResponseJsonConverter.Settings);
            responseObject.Metadata = GetResponseMetadata(response.Headers);

            return responseObject;
        }


        public async Task<Entry> GetEntryAsync(string entryId, string etag = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{Entries}/{entryId}");

            if (etag != null)
            {
                request.Headers.Add("If-None-Match", etag);
            }

            using var response = await _httpClient.SendAsync(request);

            // until we have a better error-handling mechanism we just let the code throw built-in exception if request was not successful
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsStringAsync();

            var responseObject = JsonConvert.DeserializeObject<Entry>(result, EntryResponseJsonConverter.Settings);
            responseObject.Metadata = GetResponseMetadata(response.Headers);

            return responseObject;
        }

        private static ResponseMetadata GetResponseMetadata(HttpResponseHeaders headers)
        {
            return new ResponseMetadata
            {
                ETag = headers.ETag?.Tag,
                RateLimits = new RateLimits
                {
                    DailyLimitRemaining = ParseRateLimitHeader(ResponseHeaders.HeaderDailyLimitRemaining),
                    DailyLimit = ParseRateLimitHeader(ResponseHeaders.HeaderRateLimitDailyLimit),
                    Limit = ParseRateLimitHeader(ResponseHeaders.HeaderRateLimitLimit),
                    Remaining = ParseRateLimitHeader(ResponseHeaders.HeaderRateLimitRemaining)
                }
            };

            int ParseRateLimitHeader(string header)
            {
                if (headers.TryGetValues(header, out var headerValues) && headerValues.Count() == 1)
                {
                    if (int.TryParse(headerValues.First(), out var value))
                    {
                        return value;
                    }
                }

                return -1;
            }
        }
    }
}
