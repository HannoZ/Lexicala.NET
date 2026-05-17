![Main package](https://github.com/HannoZ/Lexicala.NET/workflows/Build%20Test%20Package/badge.svg)

# Lexicala.NET

A modern .NET client library for the Lexicala Dictionary API. Lexicala provides comprehensive linguistic data including translations, definitions, pronunciations, and more across multiple languages.

## Prerequisites

- .NET 8.0 or .NET 10.0 runtime
- A RapidAPI account with an API key for Lexicala (sign up at [RapidAPI](https://rapidapi.com/))

## Installation

Install the package via NuGet:

```bash
dotnet add package Lexicala.NET
```

Or using the Package Manager Console:

```powershell
Install-Package Lexicala.NET
```

## Configuration

### 1. Obtain API Key

1. Sign up for a RapidAPI account at [rapidapi.com](https://rapidapi.com/)
2. Subscribe to the [Lexicala API](https://rapidapi.com/lexicala/api/lexicala/)
3. Copy your API key from the RapidAPI dashboard

### 2. Configure Your Application

Add the Lexicala configuration to your `appsettings.json`:

```json
{
  "Lexicala": {
        "ApiKey": "your-rapidapi-key-here",
        "UseLiteEndpoints": false
  }
}
```

Set `UseLiteEndpoints` to `true` if your subscription only allows Lite entry/sense endpoints. When enabled, the client automatically uses:

- `/search-entries-lite` instead of `/search-entries`
- `/entries-lite/{entryId}` instead of `/entries/{entryId}`
- `/senses-lite/{senseId}` instead of `/senses/{senseId}`

To restore missing translation details when using lite endpoints, use the translation-enriched methods:

- `GetEntryWithTranslationsAsync(entryId, targetLanguage)`
- `GetSenseWithTranslationsAsync(senseId, targetLanguage)`

These methods call translation endpoints to enrich missing sense and example translations.

### 3. Register Services

In your `Program.cs`:

```csharp
using Lexicala.NET;

var builder = WebApplication.CreateBuilder(args);

// Register Lexicala services
builder.Services.RegisterLexicala(builder.Configuration);

var app = builder.Build();
// ... rest of your setup
```

## Supported Frameworks

- .NET 8.0
- .NET 10.0

## Getting Started

After configuration, you can inject `ILexicalaClient` or `ILexicalaSearchParser` into your services.

### Basic Search Example

```csharp
// Inject ILexicalaClient
public class TranslationService
{
    private readonly ILexicalaClient _client;

    public TranslationService(ILexicalaClient client)
    {
        _client = client;
    }

    public async Task<string> TranslateWordAsync(string word, string fromLang, string toLang)
    {
        var searchResponse = await _client.BasicSearchAsync(word, fromLang);
        if (searchResponse.Results.Any())
        {
            var entry = await _client.GetEntryAsync(searchResponse.Results.First().Id);
            // Translations is Dictionary<string, TranslationObject> keyed by 2-letter language code
            var sense = entry.Senses.FirstOrDefault(s => s.Translations.ContainsKey(toLang));
            if (sense is not null && sense.Translations.TryGetValue(toLang, out var translationObj))
            {
                return translationObj.Translation?.Text
                    ?? translationObj.Translations?.FirstOrDefault()?.Text
                    ?? "Translation not found";
            }
        }
        return "Word not found";
    }
}
```

## Code Examples

### Get Available Languages

```csharp
var languagesResponse = await lexicalaClient.LanguagesAsync();
var globalLanguages = languagesResponse.Resources.Global;
Console.WriteLine($"Available languages: {string.Join(", ", globalLanguages.SourceLanguages)}");
```

### Basic Search

```csharp
// Search for "hello" in English
var searchResponse = await lexicalaClient.BasicSearchAsync("hello", "en");
foreach (var result in searchResponse.Results)
{
    var headwordText =
        result.Headword?.Headword?.Text
        ?? result.Headword?.HeadwordElementArray?.FirstOrDefault()?.Text
        ?? "(no headword)";

    Console.WriteLine($"Found: {headwordText} (ID: {result.Id})");
}
```

### Advanced Search

```csharp
var advancedRequest = new AdvancedSearchRequest
{
    Language = "en",
    SearchText = "run",
    Pos = "verb"  // Part of speech filter
};

var advancedResponse = await lexicalaClient.AdvancedSearchAsync(advancedRequest);
foreach (var result in advancedResponse.Results)
{
    var entry = await lexicalaClient.GetEntryAsync(result.Id);
    // Process detailed entry information
}
```

### Using the Search Parser

The `ILexicalaSearchParser` provides a higher-level abstraction for easier parsing:

```csharp
// Inject ILexicalaSearchParser
public class SearchService
{
    private readonly ILexicalaSearchParser _parser;

    public SearchService(ILexicalaSearchParser parser)
    {
        _parser = parser;
    }

    public async Task<SearchResultModel> SearchAndParseAsync(string term, string language)
    {
        return await _parser.SearchAsync(term, language);
    }
}

// Usage
var result = await searchService.SearchAndParseAsync("árbol", "es");
var englishSummary = result.Summary("en");  // "tree, shaft, post, mast"
foreach (var searchResult in result.Results)
{
    var definition = searchResult.Senses.FirstOrDefault()?.Definition;
    Console.WriteLine($"Definition: {definition}");
}
```

### Get Entry Details

```csharp
var entry = await lexicalaClient.GetEntryAsync("EN00001234");  // Example ID
foreach (var sense in entry.Senses)
{
    Console.WriteLine($"Sense: {sense.Definition}");
}
foreach (var headword in entry.Headwords)
{
    foreach (var pron in headword.Pronunciations)
    {
        Console.WriteLine($"Pronunciation: {pron.Value}");
    }
}
```

## Testing with Swagger UI

The repository includes an ASP.NET Core minimal Web API demo host with Swagger UI for testing all endpoints.

1. Clone the repository and navigate to the demo API:

   ```bash
   cd source/Demo/Lexicala.NET.Demo.Api
   ```

2. Configure your Lexicala API key. Choose one of the following:

   **Recommended: Use User Secrets**

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Lexicala:ApiKey" "your-rapidapi-key-here"
   ```

   **Alternative: Edit appsettings.json**

   Open `appsettings.json` in the demo API folder and add:

   ```json
   {
     "Lexicala": {
             "ApiKey": "your-rapidapi-key-here",
             "UseLiteEndpoints": false
     }
   }
   ```

3. Run the application:

   ```bash
   dotnet run
   ```

4. Open Swagger UI in your browser:
   - HTTP: `http://localhost:5000/swagger`
   - HTTPS: `https://localhost:5001/swagger`

5. To use the Sense Sprint web app, see the [Sense Sprint documentation](https://github.com/HannoZ/Lexicala.NET/blob/main/source/Demo/sense-sprint-web/README.md) for setup and gameplay details.

Available endpoints:

- `GET /test` - Test API connectivity
- `GET /languages` - Get available languages
- `GET /search` - Basic search
- `GET /search-entries` - Basic search with full entries
- `GET /search-entries-lite` - Basic search with full entries in lite mode (`UseLiteEndpoints=true`)
- `GET /search-rdf` - Basic search in RDF/JSON-LD format
- `GET /search-by-definitions` - Free-text search in definitions
- `GET /fluky-search` - Random word discovery
- `GET /entries/{entryId}` - Get dictionary entry by ID
- `GET /entries-lite/{entryId}` - Get dictionary entry by ID in lite mode (`UseLiteEndpoints=true`)
- `GET /senses/{senseId}` - Get sense by ID
- `GET /senses-lite/{senseId}` - Get sense by ID in lite mode (`UseLiteEndpoints=true`)
- `GET /rdf/{entryId}` - Get entry in RDF/JSON-LD format
- `POST /search-advanced` - Advanced search
- `POST /search-entries-advanced` - Advanced search with full entries
- `POST /search-rdf-advanced` - Advanced search in RDF/JSON-LD format

Missing endpoints compared to Rapid Api test console / Lexicala MCP tooling - these endpoints are NOT listed in the official documentation!:

- `GET /abbreviations`
- `GET /reverse-abbreviations`
- `GET /antonyms`
- `GET /definitions`
- `GET /examples`
- `GET /frequencies`
- `GET /phrases`
- `GET /pronunciations`
- `GET /registers`
- `GET /semantic-categories`
- `GET /subcategorizations`
- `GET /synonyms`

Implemented in this SDK (not exposed by the demo API host routes):

- `GET /translate-to`
- `GET /translate-example`
- `GET /translate-phrase`

For React frontend development, CORS is enabled for:

- `http://localhost:3000`
- `http://localhost:5173`

## Demo Game

A dedicated React + Vite frontend for a word guessing game is available at `source/Demo/sense-sprint-web`.

The web demo currently includes two game modes. `Sense Sprint` is the lower-cost mode. `Translation Quiz` makes more Lexicala calls per round because it has to source a word, fetch its entry, and build multiple distractor choices. If you are using a free evaluation subscription, use Translation Quiz sparingly.

1. Start the backend API (see Testing with Swagger UI above)

2. In another terminal, navigate to the frontend:

   ```bash
   cd source/Demo/sense-sprint-web
   ```

3. Install dependencies and start the dev server:

   **PowerShell:**

   ```powershell
   npm.cmd install
   npm.cmd run dev
   ```

   **Bash / Command Prompt:**

   ```bash
   npm install
   npm run dev
   ```

4. Open the app at `http://localhost:5173`

For complete game documentation, features, and tips, see the [Sense Sprint README](https://github.com/HannoZ/Lexicala.NET/blob/master/source/Demo/sense-sprint-web/README.md).

## Building from Source

1. Clone the repository:

   ```bash
   git clone https://github.com/HannoZ/Lexicala.NET.git
   cd Lexicala.NET
   ```

2. Build the solution:

   ```bash
    dotnet build source/Lexicala.NET.slnx
   ```

3. Run tests:

   ```bash
    dotnet test source/Lexicala.NET.slnx
   ```

The legacy `source/Lexicala.NET.sln` file has been removed in favor of `source/Lexicala.NET.slnx`.

## Supported API Values

The library validates and supports these commonly used API parameter values:

- `source` values for `FlukySearchAsync` and `AdvancedSearch*Async`: `global`, `password`, `random`, `multigloss`
- Language parameters (`language`, `sourceLanguage`) must be 2-character language codes
- `AdvancedSearchRequest.Page` accepts values up to `1000`
- `AdvancedSearchRequest.Sample` accepts values up to `1000`
- `AdvancedSearchRequest.PageLength` accepts values between `1` and `30` (default `10`)

## API Coverage

The library implements the following Lexicala API endpoints:

### Utility Endpoints

- `/test` - Test API connectivity
- `/languages` - Get available languages

### Search Endpoints

- `/search` - Basic search
- `/search-entries` - Search with full entries
- `/search-entries-lite` - Search with full entries in lite mode (`UseLiteEndpoints=true`)
- `/search-rdf` - Search in RDF/JSON-LD format
- `/search-by-definitions` - Free-text search in definitions
- `/fluky-search` - Random word discovery

### Advanced Search Endpoints

- `/search-advanced` - Advanced search with custom parameters
- `/search-entries-advanced` - Advanced search with full entries
- `/search-rdf-advanced` - Advanced search in RDF/JSON-LD format

### Entry and Sense Endpoints

- `/entries` - Get entry details by ID
- `/entries-lite` - Get entry details by ID in lite mode (`UseLiteEndpoints=true`)
- `/senses` - Get sense details by ID
- `/senses-lite` - Get sense details by ID in lite mode (`UseLiteEndpoints=true`)
- `/rdf` - Get entry in RDF/JSON-LD format

### Translation Endpoints

- `/translate-to` - Translate a lexical unit into a target language
- `/translate-example` - Translate an example sentence into a target language
- `/translate-phrase` - Translate a phrase into a target language

### Lite Translation Enrichment

- `GetEntryWithTranslationsAsync` - Retrieves an entry (including lite mode) and enriches missing sense/example translations using translation endpoints
- `GetSenseWithTranslationsAsync` - Retrieves a sense (including lite mode) and enriches missing sense/example translations using translation endpoints

For complete API documentation, visit the [Lexicala API Documentation](https://api.lexicala.com/documentation).

## Rate Limiting

The Lexicala API enforces a daily request quota. When the quota is exhausted the API returns **HTTP 429 Too Many Requests** and includes an `X-RateLimit-requests-Reset` header indicating how many seconds remain until the quota resets.

### Retry behaviour

The client uses a Polly-based retry policy with the following rules:

| Condition | Behaviour |
|-----------|-----------|
| Transient errors (5xx, network) | Retry up to 3 times with exponential back-off (max 8 s per wait) |
| HTTP 429 — reset ≤ 60 s | Retry up to 3 times, waiting the server-indicated number of seconds between attempts |
| HTTP 429 — reset > 60 s | **Fail immediately** — no retry; a `LexicalaApiException` with `StatusCode = 429` is thrown |

The 60-second threshold prevents the application from stalling when the quota won't reset for minutes or hours. In that scenario every retry attempt would also receive a 429, so the library surfaces the failure straight away instead of blocking.

### Logging

When a request fails due to rate limiting, structured log messages are emitted:

- **Warning** (before each retry): `Rate limit exceeded (HTTP 429). Waiting {n}s before retry attempt {x}/3.`
- **Warning** (skip-retry path): `Rate limit exceeded (HTTP 429). API quota resets in {n}s which exceeds the retry threshold (60s). Not retrying.`
- **Error** (after final failure): `API rate limit exceeded (HTTP 429). Quota resets in {n}s. Request failed without retrying because the reset time exceeds the retry threshold.`

### Handling the exception

```csharp
try
{
    var result = await client.BasicSearchAsync("hello", "en");
}
catch (LexicalaApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
{
    var secondsUntilReset = ex.Metadata?.RateLimits?.Reset;
    Console.WriteLine($"Daily quota exceeded. Quota resets in {secondsUntilReset}s.");
}
```

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
