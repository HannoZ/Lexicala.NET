﻿using Lexicala.NET.Client.Request;
using Lexicala.NET.Client.Response;
using Lexicala.NET.Client.Response.Entries;
using Lexicala.NET.Client.Response.Entries.JsonConverters;
using Lexicala.NET.Client.Response.Search;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lexicala.NET.Client.Response.Languages;
using Lexicala.NET.Client.Response.Me;
using Lexicala.NET.Client.Response.Test;

namespace Lexicala.NET.Client
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

        public async Task<SearchResponse> BasicSearchAsync(string searchText, string sourceLanguage, string etag = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Search}?language={sourceLanguage}&text={searchText}");
            if (etag != null)
            {
                request.Headers.Add("If-None-Match", etag);
            }

            var response = await _httpClient.SendAsync(request);
            
            string result = await response.Content.ReadAsStringAsync();

            var responseObject = JsonConvert.DeserializeObject<SearchResponse>(result, SearchResponseJsonConverter.Settings);
            responseObject.Metadata = GetResponseMetadata(response.Headers);
            
            return responseObject;
        }

        /// <summary>
        /// TODO - Implement
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest request)
        {
            return Task.FromResult(new SearchResponse());
        }

        public async Task<Entry> GetEntryAsync(string entryId, string etag = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Entries}/{entryId}");
            var response = await _httpClient.SendAsync(request);
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
