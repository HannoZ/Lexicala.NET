using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Languages;
using Lexicala.NET.Response.Me;
using Lexicala.NET.Response.Search;
using Lexicala.NET.Response.Test;
using Microsoft.Extensions.Logging;
using Sense = Lexicala.NET.Response.Entries.Sense;

namespace Lexicala.NET
{
    /// <inheritdoc cref="ILexicalaClient" />
    public class LexicalaClient : ILexicalaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LexicalaClient> _logger;

        private const int ExcessiveThreshold = 1000;

        /// <summary>
        /// Creates a new instance of the <see cref="LexicalaClient"/> class.
        /// </summary>
        /// <remarks>
        /// This class should not be instantiated directly, but registered as implementation of the <see cref="ILexicalaClient"/> interface in the dependency injection framework.
        /// </remarks>
        public LexicalaClient(HttpClient httpClient, ILogger<LexicalaClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(HttpMethod method, string endpoint, string etag = null, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(method, endpoint);
            AddETagIfPresent(etag, request);

            _logger.LogDebug("Executing {Method} request to {Endpoint}", method, endpoint);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Request to {Endpoint} failed with status code {StatusCode}", endpoint, response.StatusCode);
                throw await CreateApiExceptionAsync(response, cancellationToken);
            }

            _logger.LogDebug("Request to {Endpoint} succeeded with status code {StatusCode}", endpoint, response.StatusCode);
            return response;
        }

        /// <inheritdoc />
        public async Task<TestResponse> TestAsync(CancellationToken cancellationToken = default)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, Constants.Test, cancellationToken: cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TestResponse>(content, JsonSerializerDefaults.Options);
        }
        
        /// <inheritdoc />
        public async Task<LanguagesResponse> LanguagesAsync(CancellationToken cancellationToken = default)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, Constants.Languages, cancellationToken: cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<LanguagesResponse>(content, JsonSerializerDefaults.Options);
        }

        /// <inheritdoc />
        public Task<SearchResponse> BasicSearchAsync(string searchText, string sourceLanguage, string etag = null, CancellationToken cancellationToken = default)
        {
            ValidateLanguageCode(sourceLanguage, nameof(sourceLanguage));
            ValidateSearchText(searchText, nameof(searchText));

            _logger.LogInformation("Performing basic search for text '{SearchText}' in language '{SourceLanguage}'", searchText, sourceLanguage);

            var query = $"{Constants.Search}?language={Uri.EscapeDataString(sourceLanguage)}&text={Uri.EscapeDataString(searchText)}";
            return ExecuteSearch(query, etag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            ValidateSearchRequest(searchRequest);

            _logger.LogInformation("Performing advanced search for text '{SearchText}' in language '{SourceLanguage}' with parameters: synonyms={Synonyms}, antonyms={Antonyms}",
                searchRequest.SearchText, searchRequest.Language, searchRequest.Synonyms, searchRequest.Antonyms);

            var queryString = BuildAdvancedSearchQueryString(Constants.Search, searchRequest);
            return ExecuteSearch(queryString, searchRequest.ETag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Entry>> SearchEntriesAsync(string searchText, string sourceLanguage, string etag = null, CancellationToken cancellationToken = default)
        {
            ValidateLanguageCode(sourceLanguage, nameof(sourceLanguage));
            ValidateSearchText(searchText, nameof(searchText));

            var queryString = $"{Constants.SearchEntries}?language={Uri.EscapeDataString(sourceLanguage)}&text={Uri.EscapeDataString(searchText)}";
            return ExecuteSearchEntries(queryString, etag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Entry>> AdvancedSearchEntriesAsync(AdvancedSearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            ValidateSearchRequest(searchRequest);

            var queryString = BuildAdvancedSearchQueryString(Constants.SearchEntries, searchRequest);
            return ExecuteSearchEntries(queryString, searchRequest.ETag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> SearchRdfAsync(string searchText, string sourceLanguage, string etag = null, CancellationToken cancellationToken = default)
        {
            ValidateLanguageCode(sourceLanguage, nameof(sourceLanguage));
            ValidateSearchText(searchText, nameof(searchText));

            var query = $"{Constants.SearchRdf}?language={Uri.EscapeDataString(sourceLanguage)}&text={Uri.EscapeDataString(searchText)}";
            return ExecuteRdfQuery(query, etag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> AdvancedSearchRdfAsync(AdvancedSearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            ValidateSearchRequest(searchRequest);

            var queryString = BuildAdvancedSearchQueryString(Constants.SearchRdf, searchRequest);
            return ExecuteRdfQuery(queryString, searchRequest.ETag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> GetRdfAsync(string entryId, string etag = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(entryId, nameof(entryId));
            return ExecuteRdfQuery($"{Constants.Rdf}/{Uri.EscapeDataString(entryId)}", etag, cancellationToken);
        }

        private static string BuildAdvancedSearchQueryString(string endpoint, AdvancedSearchRequest searchRequest)
        {
            // build the querystring based on provided search request params
            var queryStringBuilder = new StringBuilder($"{endpoint}?language={Uri.EscapeDataString(searchRequest.Language)}&text={Uri.EscapeDataString(searchRequest.SearchText)}");
            queryStringBuilder.Append("&source=" + Uri.EscapeDataString(searchRequest.Source));
            
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
            if (searchRequest.Synonyms)
            {
                queryStringBuilder.Append("&synonyms=true");
            }
            if (searchRequest.Antonyms)
            {
                queryStringBuilder.Append("&antonyms=true");
            }
            if (!string.IsNullOrEmpty(searchRequest.Pos))
            {
                queryStringBuilder.Append("&pos=" + Uri.EscapeDataString(searchRequest.Pos));
            }
            if (!string.IsNullOrEmpty(searchRequest.Number))
            {
                queryStringBuilder.Append("&number=" + Uri.EscapeDataString(searchRequest.Number));
            }
            if (!string.IsNullOrEmpty(searchRequest.Gender))
            {
                queryStringBuilder.Append("&gender=" + Uri.EscapeDataString(searchRequest.Gender));
            }
            if (!string.IsNullOrEmpty(searchRequest.Subcategorization))
            {
                queryStringBuilder.Append("&subcategorization=" + Uri.EscapeDataString(searchRequest.Subcategorization));
            }

            // pagination - only append if values are other than default values and within reasonable bounds
            if (searchRequest.Page > 1 && searchRequest.Page <= ExcessiveThreshold) // Prevent excessive page numbers
            {
                queryStringBuilder.Append("&page=" + searchRequest.Page);
            }
            if (searchRequest.PageLength != 10 && searchRequest.PageLength is > 0 and <= 30)
            {
                queryStringBuilder.Append("&page-length=" + searchRequest.PageLength);
            }
            if (searchRequest.Sample > 0 && searchRequest.Sample <= ExcessiveThreshold) // Prevent excessive sampling
            {
                queryStringBuilder.Append("&sample=" + searchRequest.Sample);
            }

            return queryStringBuilder.ToString();
        }

        private static void ValidateSearchRequest(AdvancedSearchRequest searchRequest)
        {
            ArgumentNullException.ThrowIfNull(searchRequest);
            ValidateLanguageCode(searchRequest.Language, nameof(searchRequest.Language));
            ValidateSearchText(searchRequest.SearchText, nameof(searchRequest.SearchText));
        }

        private static void ValidateLanguageCode(string languageCode, string parameterName)
        {
            ArgumentException.ThrowIfNullOrEmpty(languageCode, parameterName);
            if (languageCode.Length != 2)
            {
                throw new ArgumentException($"Invalid language code provided ({languageCode}), a valid language code is two characters", parameterName);
            }
        }

        private static void ValidateSearchText(string searchText, string parameterName)
        {
            ArgumentException.ThrowIfNullOrEmpty(searchText, parameterName);
        }


        private async Task<SearchResponse> ExecuteSearch(string querystring, string etag, CancellationToken cancellationToken)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, querystring, etag, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<SearchResponse>(content, JsonSerializerDefaults.Options);
            responseObject.Metadata = GetResponseMetadata(response.Headers);
            return responseObject;
        }

        private async Task<IEnumerable<Entry>> ExecuteSearchEntries(string querystring, string etag, CancellationToken cancellationToken)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, querystring, etag, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var entries = JsonSerializer.Deserialize<IEnumerable<Entry>>(content, JsonSerializerDefaults.Options);
            // Note: Metadata is per entry, but since it's a collection, perhaps set on each or return as is
            // For simplicity, return the entries; metadata can be handled differently if needed
            return entries;
        }

        private async Task<string> ExecuteRdfQuery(string querystring, string etag, CancellationToken cancellationToken)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, querystring, etag, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Entry> GetEntryAsync(string entryId, string etag = null, CancellationToken cancellationToken = default)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, $"{Constants.Entries}/{entryId}", etag, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<Entry>(content, JsonSerializerDefaults.Options);
            responseObject.Metadata = GetResponseMetadata(response.Headers);
            return responseObject;
        }

        /// <inheritdoc />
        public async Task<Sense> GetSenseAsync(string senseId, string etag = null, CancellationToken cancellationToken = default)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, $"{Constants.Senses}/{senseId}", etag, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<Sense>(content, JsonSerializerDefaults.Options);
            responseObject.Metadata = GetResponseMetadata(response.Headers);
            return responseObject;
        }

        /// <inheritdoc />
        public Task<SearchResponse> SearchDefinitionsAsync(string searchText, string language = null, string etag = null, CancellationToken cancellationToken = default)
        {
            ValidateSearchText(searchText, nameof(searchText));

            _logger.LogInformation("Performing definitions search for text '{SearchText}' with language filter '{Language}'", searchText, language);

            var query = $"{Constants.SearchDefinitions}?text={Uri.EscapeDataString(searchText)}";
            if (!string.IsNullOrEmpty(language))
            {
                ValidateLanguageCode(language, nameof(language));
                query += $"&lang={Uri.EscapeDataString(language)}";
            }

            return ExecuteSearch(query, etag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<SearchResponse> FlukySearchAsync(string source = "global", string language = null, string etag = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Performing fluky search in source '{Source}' with language '{Language}'", source, language ?? "random");

            var query = $"{Constants.FlukySearch}?source={Uri.EscapeDataString(source)}";
            if (!string.IsNullOrEmpty(language))
            {
                ValidateLanguageCode(language, nameof(language));
                query += $"&language={Uri.EscapeDataString(language)}";
            }

            return ExecuteSearch(query, etag, cancellationToken);
        }

        private static void AddETagIfPresent(string etag, HttpRequestMessage request)
        {
            if (etag != null)
            {
                request.Headers.Add("If-None-Match", etag);
            }
        }

        private static ResponseMetadata GetResponseMetadata(HttpResponseHeaders headers)
        {
            return new ResponseMetadata
            {
                ETag = headers.ETag?.Tag,
                RateLimits = new RateLimits
                {
                    LimitRemaining = ParseRateLimitHeader(ResponseHeaders.HeaderRateLimitRequestsRemaining),
                    Limit = ParseRateLimitHeader(ResponseHeaders.HeaderRateLimitRequestsLimit),
                    Reset = ParseRateLimitHeader(ResponseHeaders.HeaderRateLimitReset)
                }
            };

            int ParseRateLimitHeader(string header)
            {
                if (headers.TryGetValues(header, out var headerValues))
                {
                    foreach (var headerValue in headerValues)
                    {
                        if (int.TryParse(headerValue, out var value))
                        {
                            return value;
                        }
                    }
                }

                return -1;
            }
        }

        private async Task<LexicalaApiException> CreateApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var message = GetErrorMessageFromContent(content) ?? response.ReasonPhrase ?? "An error occurred while calling the Lexicala API.";

            _logger.LogError("API request failed with status {StatusCode}. Error message: {Message}", response.StatusCode, message);

            return new LexicalaApiException(message, response.StatusCode, content, GetResponseMetadata(response.Headers));
        }

        private static string GetErrorMessageFromContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (TryGetString(root, "message", out var message) ||
                    TryGetString(root, "error", out message) ||
                    TryGetString(root, "error_description", out message))
                {
                    return message;
                }

                if (root.TryGetProperty("error", out var errorProperty) && errorProperty.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetString(errorProperty, "message", out var nestedMessage))
                    {
                        return nestedMessage;
                    }
                }
            }
            catch (JsonException)
            {
                // Return a generic message for malformed JSON instead of silently ignoring
                return "The API returned a response that could not be parsed. The response may contain invalid JSON.";
            }

            return null;

            static bool TryGetString(JsonElement element, string propertyName, out string value)
            {
                if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                {
                    value = property.GetString();
                    return true;
                }

                value = null;
                return false;
            }
        }
    }
}
