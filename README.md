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
    "ApiKey": "your-rapidapi-key-here"
  }
}
```

### 3. Register Services
In your `Program.cs` (for .NET 6+):

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
            var sense = entry.Senses.FirstOrDefault(s => s.Translations?.Any(t => t.Language == toLang) == true);
            return sense?.Translations?.FirstOrDefault(t => t.Language == toLang)?.Text ?? "Translation not found";
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
Console.WriteLine($"Available languages: {string.Join(", ", globalLanguages)}");
```

### Basic Search

```csharp
// Search for "hello" in English
var searchResponse = await lexicalaClient.BasicSearchAsync("hello", "en");
foreach (var result in searchResponse.Results)
{
    Console.WriteLine($"Found: {result.Headword?.Text} (ID: {result.Id})");
}
```

### Advanced Search

```csharp
var advancedRequest = new AdvancedSearchRequest
{
    Source = "en",
    Text = "run",
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
    var definition = searchResult.Senses.FirstOrDefault()?.Definition?.Text;
    Console.WriteLine($"Definition: {definition}");
}
```

### Get Entry Details

```csharp
var entry = await lexicalaClient.GetEntryAsync("EN00001234");  // Example ID
foreach (var sense in entry.Senses)
{
    Console.WriteLine($"Sense: {sense.Definition?.Text}");
    if (sense.Pronunciations != null)
    {
        foreach (var pron in sense.Pronunciations)
        {
            Console.WriteLine($"Pronunciation: {pron.Value}");
        }
    }
}
```

## Testing with Swagger UI

The repository includes a console application that hosts a Web API with Swagger UI for testing all endpoints.

1. Clone the repository and navigate to the console app:
   ```bash
   cd source/Lexicala.NET.ConsoleApp
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open Swagger UI in your browser:
   - HTTP: `http://localhost:5000/swagger`
   - HTTPS: `https://localhost:5001/swagger`

4. Test the endpoints directly in the UI.

Available endpoints:
- `GET /test` - Test API connectivity
- `GET /languages` - Get available languages
- `GET /search` - Basic search
- `GET /search-entries` - Basic search with full entries
- `GET /search-rdf` - Basic search in RDF/JSON-LD format
- `GET /search-definitions` - Free-text search in definitions
- `GET /fluky-search` - Random word discovery
- `GET /entry/{entryId}` - Get dictionary entry by ID
- `GET /sense/{senseId}` - Get sense by ID
- `GET /rdf/{entryId}` - Get entry in RDF/JSON-LD format
- `POST /search-advanced` - Advanced search
- `POST /search-entries-advanced` - Advanced search with full entries
- `POST /search-rdf-advanced` - Advanced search in RDF/JSON-LD format

## Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/HannoZ/Lexicala.NET.git
   cd Lexicala.NET
   ```

2. Build the solution:
   ```bash
   dotnet build source/Lexicala.NET.sln
   ```

3. Run tests:
   ```bash
   dotnet test source/Lexicala.NET.sln
   ```

## API Coverage

The library implements the following Lexicala API endpoints:

**Utility Endpoints**
- `/test` - Test API connectivity
- `/languages` - Get available languages

**Search Endpoints**
- `/search` - Basic search
- `/search-entries` - Search with full entries
- `/search-rdf` - Search in RDF/JSON-LD format
- `/search-definitions` - Free-text search in definitions
- `/fluky-search` - Random word discovery

**Advanced Search Endpoints**
- `/search-advanced` - Advanced search with custom parameters
- `/search-entries-advanced` - Advanced search with full entries
- `/search-rdf-advanced` - Advanced search in RDF/JSON-LD format

**Entry and Sense Endpoints**
- `/entries` - Get entry details by ID
- `/senses` - Get sense details by ID
- `/rdf` - Get entry in RDF/JSON-LD format

For complete API documentation, visit the [Lexicala API Documentation](https://api.lexicala.com/documentation).

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 
