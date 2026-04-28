using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Languages;
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
            ArgumentException.ThrowIfNullOrEmpty(searchText, nameof(searchText));

            _logger.LogDebug("Performing basic search for text '{SearchText}' in language '{SourceLanguage}'", searchText, sourceLanguage);

            var query = $"{Constants.Search}?language={Uri.EscapeDataString(sourceLanguage)}&text={Uri.EscapeDataString(searchText)}";
            return ExecuteSearch(query, etag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<SearchResponse> AdvancedSearchAsync(AdvancedSearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            ValidateSearchRequest(searchRequest);

            _logger.LogDebug("Performing advanced search for text '{SearchText}' in language '{SourceLanguage}' with parameters: synonyms={Synonyms}, antonyms={Antonyms}",
                searchRequest.SearchText, searchRequest.Language, searchRequest.Synonyms, searchRequest.Antonyms);

            var queryString = BuildAdvancedSearchQueryString(Constants.Search, searchRequest);
            return ExecuteSearch(queryString, searchRequest.ETag, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Entry>> SearchEntriesAsync(string searchText, string sourceLanguage, string etag = null, CancellationToken cancellationToken = default)
        {
            ValidateLanguageCode(sourceLanguage, nameof(sourceLanguage));
            ArgumentException.ThrowIfNullOrEmpty(searchText, nameof(searchText));

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
            ArgumentException.ThrowIfNullOrEmpty(searchText, nameof(searchText));

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

        /// <inheritdoc />
        public async Task<Entry> GetEntryAsync(string entryId, string etag = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(entryId, nameof(entryId));
            using var response = await ExecuteRequestAsync(HttpMethod.Get, $"{Constants.Entries}/{Uri.EscapeDataString(entryId)}", etag, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<Entry>(content, JsonSerializerDefaults.Options);
            responseObject.Metadata = GetResponseMetadata(response.Headers);
            return responseObject;
        }

        /// <inheritdoc />
        public async Task<Sense> GetSenseAsync(string senseId, string etag = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(senseId, nameof(senseId));
            using var response = await ExecuteRequestAsync(HttpMethod.Get, $"{Constants.Senses}/{Uri.EscapeDataString(senseId)}", etag, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<Sense>(content, JsonSerializerDefaults.Options);
            responseObject.Metadata = GetResponseMetadata(response.Headers);
            return responseObject;
        }

        /// <inheritdoc />
        public Task<SearchResponse> SearchDefinitionsAsync(string searchText, string language = null, string etag = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(searchText, nameof(searchText));

            _logger.LogDebug("Performing definitions search for text '{SearchText}' with language filter '{Language}'", searchText, language);

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
            ValidateSource(source, nameof(source));

            _logger.LogDebug("Performing fluky search in source '{Source}' with language '{Language}'", source, language ?? "random");

            var query = $"{Constants.FlukySearch}?source={Uri.EscapeDataString(source)}";
            if (!string.IsNullOrEmpty(language))
            {
                ValidateLanguageCode(language, nameof(language));
                query += $"&language={Uri.EscapeDataString(language)}";
            }

            return ExecuteSearch(query, etag, cancellationToken);
        }

        private static string BuildAdvancedSearchQueryString(string endpoint, AdvancedSearchRequest searchRequest)
        {
            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new("language", searchRequest.Language),
                new("text", searchRequest.SearchText),
                new("source", searchRequest.Source)
            };

            // Add optional boolean search criteria flags only when explicitly enabled.
            AddIfTrue(searchRequest.Analyzed, "analyzed");
            AddIfTrue(searchRequest.Monosemous, "monosemous");
            AddIfTrue(searchRequest.Polysemous, "polysemous");
            AddIfTrue(searchRequest.Morph, "morph");
            AddIfTrue(searchRequest.Synonyms, "synonyms");
            AddIfTrue(searchRequest.Antonyms, "antonyms");

            // Add optional string filters only when provided.
            AddIfNotEmpty(searchRequest.Pos, "pos");
            AddIfNotEmpty(searchRequest.Number, "number");
            AddIfNotEmpty(searchRequest.Gender, "gender");
            AddIfNotEmpty(searchRequest.Subcategorization, "subcategorization");

            // Enforce existing pagination constraints.
            if (searchRequest.Page is > 1 and <= Constants.MaxRequestThreshold)
            {
                queryParameters.Add(new KeyValuePair<string, string>("page", searchRequest.Page.ToString()));
            }

            if (searchRequest.PageLength != 10 && searchRequest.PageLength is > 0 and <= 30)
            {
                queryParameters.Add(new KeyValuePair<string, string>("page-length", searchRequest.PageLength.ToString()));
            }

            if (searchRequest.Sample is > 0 and <= Constants.MaxRequestThreshold)
            {
                queryParameters.Add(new KeyValuePair<string, string>("sample", searchRequest.Sample.ToString()));
            }

            return BuildQueryString(endpoint, queryParameters);

            void AddIfTrue(bool include, string key)
            {
                if (include)
                {
                    queryParameters.Add(new KeyValuePair<string, string>(key, "true"));
                }
            }

            void AddIfNotEmpty(string value, string key)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    queryParameters.Add(new KeyValuePair<string, string>(key, value));
                }
            }
        }

        private static string BuildQueryString(string endpoint, IEnumerable<KeyValuePair<string, string>> queryParameters)
        {
            var encodedPairs = queryParameters
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}");

            return $"{endpoint}?{string.Join("&", encodedPairs)}";
        }

        private static readonly HashSet<string> ValidSources = new(StringComparer.OrdinalIgnoreCase)
        {
            Sources.Global, Sources.Password, Sources.Random, Sources.Multigloss
        };

        private static void ValidateSource(string source, string parameterName)
        {
            ArgumentException.ThrowIfNullOrEmpty(source, parameterName);
            if (!ValidSources.Contains(source))
            {
                throw new ArgumentException($"Invalid source provided ({source}), valid values are: {string.Join(", ", ValidSources)}", parameterName);
            }
        }

        private static void ValidateSearchRequest(AdvancedSearchRequest searchRequest)
        {
            ArgumentNullException.ThrowIfNull(searchRequest);
            ValidateLanguageCode(searchRequest.Language, nameof(searchRequest.Language));
            ArgumentException.ThrowIfNullOrEmpty(searchRequest.SearchText, nameof(searchRequest.SearchText));
            ValidateSource(searchRequest.Source, nameof(searchRequest.Source));
        }

        private static void ValidateLanguageCode(string languageCode, string parameterName)
        {
            ArgumentException.ThrowIfNullOrEmpty(languageCode, parameterName);
            if (languageCode.Length != 2)
            {
                throw new ArgumentException($"Invalid language code provided ({languageCode}), a valid language code is two characters", parameterName);
            }
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


        private async Task<SearchResponse> ExecuteSearch(string querystring, string etag, CancellationToken cancellationToken)
        {
            using var response = await ExecuteRequestAsync(HttpMethod.Get, querystring, etag, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObject = DeserializeSearchResponse(content);
            responseObject.Metadata = GetResponseMetadata(response.Headers);
            return responseObject;
        }

        private static SearchResponse DeserializeSearchResponse(string content)
        {
            var responseObject = JsonSerializer.Deserialize<SearchResponse>(content, JsonSerializerDefaults.Options);
            if (responseObject?.Results is { Length: > 0 })
            {
                return responseObject;
            }

            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object &&
                    !root.TryGetProperty("results", out _) &&
                    root.TryGetProperty("id", out _))
                {
                    var singleResult = JsonSerializer.Deserialize<Result>(content, JsonSerializerDefaults.Options);
                    if (singleResult?.Id != null)
                    {
                        return new SearchResponse
                        {
                            NResults = 1,
                            PageNumber = 1,
                            ResultsPerPage = 1,
                            NPages = 1,
                            AvailableNPages = 1,
                            Results = [singleResult]
                        };
                    }
                }
            }
            catch (JsonException)
            {
                // Keep original deserialization result for malformed content.
            }

            return responseObject ?? new SearchResponse();
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

        private static void AddETagIfPresent(string etag, HttpRequestMessage request)
        {
            if (etag != null)
            {
                request.Headers.Add("If-None-Match", etag);
            }
        }

        /// <summary>
        /// Extracts response metadata from HTTP response headers, including ETag and rate limit information.
        /// </summary>
        /// <param name="headers">The HTTP response headers to parse</param>
        /// <returns>A ResponseMetadata object containing ETag and rate limit information. Rate limit values are set to -1 if the corresponding headers are missing or unparseable.</returns>
        /// <remarks>
        /// This method extracts:
        /// - ETag header for caching purposes
        /// - Rate limit headers (limit, remaining, reset) from the API response
        /// 
        /// If any rate limit headers are missing or contain unparseable values, they are set to -1 and a warning is logged.
        /// Callers should check for -1 values when rate limit information is required for decision-making.
        /// </remarks>
        private ResponseMetadata GetResponseMetadata(HttpResponseHeaders headers)
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

            // Parses a rate limit header value from the API response.
            // Returns the parsed integer value, or -1 if the header is missing or cannot be parsed.
            int ParseRateLimitHeader(string header)
            {
                if (headers.TryGetValues(header, out var headerValues))
                {
                    var headerValuesList = headerValues.ToList();
                    foreach (var headerValue in headerValuesList)
                    {
                        if (int.TryParse(headerValue, out var value))
                        {
                            return value;
                        }
                    }
                    
                    // Header exists but value couldn't be parsed
                    _logger.LogWarning("Rate limit header '{HeaderName}' exists but contains unparseable values: {HeaderValue}", header, string.Join(", ", headerValuesList));
                }
                else
                {
                    // Header is missing from the response
                    _logger.LogWarning("Rate limit header '{HeaderName}' is missing from the API response. This may indicate an API issue or version mismatch.", header);
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
                // Parse JSON response and attempt to extract error message from multiple possible locations
                // Different API endpoints and error conditions may use different error response formats
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                // Try primary error message fields in order of preference:
                // "message" - standard error message field
                // "error" - OAuth/standard error field
                // "error_description" - OAuth error description field
                if (TryGetString(root, "message", out var message) ||
                    TryGetString(root, "error", out message) ||
                    TryGetString(root, "error_description", out message))
                {
                    return message;
                }

                // Check for nested error object with message (some error responses have nested structure)
                // Example: { "error": { "message": "Error details" } }
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
