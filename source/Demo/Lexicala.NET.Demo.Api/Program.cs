using Lexicala.NET;
using Lexicala.NET.Parsing;
using Lexicala.NET.Demo.Api.Game;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lexicala.NET.Request;
using System.Threading;
using System.Threading.Tasks;

namespace Lexicala.NET.Demo.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddCommandLine(args)
                .AddUserSecrets<Program>();

            builder.Services.RegisterLexicala(builder.Configuration);
            builder.Services.AddSingleton<ISenseSprintGameService, SenseSprintGameService>();
            builder.Services.AddSingleton<ITranslationQuizGameService, TranslationQuizGameService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactDev", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:3000", "http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Lexicala API",
                    Version = "v1",
                    Description = "Explore the Lexicala dictionary API \u2014 search headwords, retrieve entries and senses, browse definitions, and discover random words across 25+ languages. See the <a href=\"https://api.lexicala.com/documentation/\">official Lexicala API documentation</a> for full details."
                });
                options.CustomSchemaIds(type => type.FullName);
                foreach (var xmlFile in new[] { "Lexicala.NET.Demo.Api.xml", "Lexicala.NET.xml" })
                {
                    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (System.IO.File.Exists(xmlPath))
                        options.IncludeXmlComments(xmlPath);
                }
            });
            builder.Services.AddLogging(cfg => cfg.AddConsole());

            var app = builder.Build();

            app.UseCors("ReactDev");
            app.UseDefaultFiles();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapGet("/test", async (ILexicalaClient client, CancellationToken cancellationToken) =>
                await client.TestAsync(cancellationToken))
                .WithName("Test")
                .WithTags("Health")
                .WithSummary("Health check")
                .WithDescription("Tests that the API is reachable and returns a status message.");

            app.MapGet("/languages", async (ILexicalaClient client, CancellationToken cancellationToken) =>
                await client.LanguagesAsync(cancellationToken))
                .WithName("Languages")
                .WithTags("Languages")
                .WithSummary("List available languages")
                .WithDescription("Returns information about all languages and resources available through the API, including the Global, Password, and Random House Webster's series.");

            app.MapGet("/search", async (ILexicalaClient client, string text, string language, string? etag, CancellationToken cancellationToken) =>
                await client.BasicSearchAsync(text, language, etag, cancellationToken))
                .WithName("BasicSearch")
                .WithTags("Search")
                .WithSummary("Search by headword")
                .WithDescription("Search for entries in the Global source by headword. Returns partial lexical information. Use /entry/{entryId} to retrieve a full entry.");

            app.MapGet("/search-entries", async (ILexicalaClient client, string text, string language, string? etag, CancellationToken cancellationToken) =>
                await client.SearchEntriesAsync(text, language, etag, cancellationToken))
                .WithName("SearchEntries")
                .WithTags("Search")
                .WithSummary("Search by headword — full entries")
                .WithDescription("Search for entries in the Global source by headword and return complete entry objects, including all syntactic and semantic details.");

            app.MapGet("/search-rdf", async (ILexicalaClient client, string text, string language, string? etag, CancellationToken cancellationToken) =>
                Results.Text(await client.SearchRdfAsync(text, language, etag, cancellationToken), "application/ld+json"))
                .WithName("SearchRdf")
                .WithTags("Search")
                .WithSummary("Search entries in RDF/JSON-LD format")
                .WithDescription("Search for entries by headword and return results serialised as RDF/JSON-LD.");

            app.MapGet("/entry/{entryId}", async (ILexicalaClient client, string entryId, string? etag, CancellationToken cancellationToken) =>
                await client.GetEntryAsync(entryId, etag, cancellationToken))
                .WithName("GetEntry")
                .WithTags("Entries")
                .WithSummary("Get entry by ID")
                .WithDescription("Retrieves a full dictionary entry by its ID, including syntactic and semantic information, compositional phrases, usage examples, and translations.");

            app.MapGet("/sense/{senseId}", async (ILexicalaClient client, string senseId, string? etag, CancellationToken cancellationToken) =>
                await client.GetSenseAsync(senseId, etag, cancellationToken))
                .WithName("GetSense")
                .WithTags("Entries")
                .WithSummary("Get sense by ID")
                .WithDescription("Retrieves a single sense by its unique sense ID. Sense IDs are returned as part of search results.");

            app.MapGet("/rdf/{entryId}", async (ILexicalaClient client, string entryId, string? etag, CancellationToken cancellationToken) =>
                Results.Text(await client.GetRdfAsync(entryId, etag, cancellationToken), "application/ld+json"))
                .WithName("GetRdf")
                .WithTags("Entries")
                .WithSummary("Get entry in RDF/JSON-LD format")
                .WithDescription("Retrieves a dictionary entry by ID and returns it serialised as RDF/JSON-LD.");

            app.MapPost("/search-advanced", async (ILexicalaClient client, AdvancedSearchRequest request, CancellationToken cancellationToken) =>
                await client.AdvancedSearchAsync(request, cancellationToken))
                .WithName("AdvancedSearch")
                .WithTags("Advanced Search")
                .WithSummary("Advanced search")
                .WithDescription("Search for entries using flexible filter parameters such as part of speech, gender, number, morphological analysis, synonyms, antonyms, and pagination.");

            app.MapPost("/search-entries-advanced", async (ILexicalaClient client, AdvancedSearchRequest request, CancellationToken cancellationToken) =>
                await client.AdvancedSearchEntriesAsync(request, cancellationToken))
                .WithName("AdvancedSearchEntries")
                .WithTags("Advanced Search")
                .WithSummary("Advanced search — full entries")
                .WithDescription("Search for entries using advanced filter parameters and return complete entry objects.");

            app.MapPost("/search-rdf-advanced", async (ILexicalaClient client, AdvancedSearchRequest request, CancellationToken cancellationToken) =>
                Results.Text(await client.AdvancedSearchRdfAsync(request, cancellationToken), "application/ld+json"))
                .WithName("AdvancedSearchRdf")
                .WithTags("Advanced Search")
                .WithSummary("Advanced search in RDF/JSON-LD format")
                .WithDescription("Search for entries using advanced filter parameters and return results serialised as RDF/JSON-LD.");

            app.MapGet("/search-definitions", async (ILexicalaClient client, string text, string? language, string? etag, CancellationToken cancellationToken) =>
                await client.SearchDefinitionsAsync(text, language, etag, cancellationToken))
                .WithName("SearchDefinitions")
                .WithTags("Definitions")
                .WithSummary("Search by definition text")
                .WithDescription("Performs a full-text search in definitions across 20+ supported languages (ar, cs, da, de, el, en, es, fr, he, hi, it, ja, ko, nl, no, pl, pt, ru, sv, tr). Optionally filter results by language code.");

            app.MapGet("/fluky-search", async (ILexicalaClient client, string? source, string? language, string? etag, CancellationToken cancellationToken) =>
                await client.FlukySearchAsync(source ?? "global", language, etag, cancellationToken))
                .WithName("FlukySearch")
                .WithTags("Discovery")
                .WithSummary("Random word discovery")
                .WithDescription("Returns a randomly selected entry for word discovery. Filter by source (global, password, multigloss, random) and optionally by language code.");

            app.MapPost("/game/sense-sprint/rounds", async (ISenseSprintGameService gameService, CreateRoundRequest? request, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.CreateRoundAsync(request?.Language, cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
                    }
                })
                .WithName("SenseSprintCreateRound")
                .ExcludeFromDescription();

            app.MapPost("/game/sense-sprint/rounds/{roundId:guid}/clues/next", async (ISenseSprintGameService gameService, Guid roundId, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.RevealNextClueAsync(roundId, cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        return Results.NotFound(new ProblemDetails { Title = "Round not found", Detail = ex.Message });
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new ProblemDetails { Title = "Round is not active", Detail = ex.Message });
                    }
                })
                .WithName("SenseSprintNextClue")
                .ExcludeFromDescription();

            app.MapPost("/game/sense-sprint/rounds/{roundId:guid}/guess", async (ISenseSprintGameService gameService, Guid roundId, GuessRequest request, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.SubmitGuessAsync(roundId, request.Guess, cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        return Results.NotFound(new ProblemDetails { Title = "Round not found", Detail = ex.Message });
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new ProblemDetails { Title = "Invalid guess", Detail = ex.Message });
                    }
                })
                .WithName("SenseSprintSubmitGuess")
                .ExcludeFromDescription();

            app.MapPost("/game/sense-sprint/rounds/{roundId:guid}/give-up", async (ISenseSprintGameService gameService, Guid roundId, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.GiveUpAsync(roundId, cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        return Results.NotFound(new ProblemDetails { Title = "Round not found", Detail = ex.Message });
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new ProblemDetails { Title = "Round is not active", Detail = ex.Message });
                    }
                })
                .WithName("SenseSprintGiveUp")
                .ExcludeFromDescription();

            app.MapPost("/game/translation-quiz/rounds", async (ITranslationQuizGameService gameService, string? targetLanguage, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.CreateRoundAsync(targetLanguage, cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
                    }
                })
                .WithName("TranslationQuizCreateRound")
                .ExcludeFromDescription();

            app.MapPost("/game/translation-quiz/rounds/{roundId:guid}/answer", async (ITranslationQuizGameService gameService, Guid roundId, QuizAnswerRequest request, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.SubmitAnswerAsync(roundId, request.Choice, cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        return Results.NotFound(new ProblemDetails { Title = "Round not found", Detail = ex.Message });
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new ProblemDetails { Title = "Invalid answer", Detail = ex.Message });
                    }
                })
                .WithName("TranslationQuizSubmitAnswer")
                .ExcludeFromDescription();

            app.MapPost("/game/translation-quiz/rounds/{roundId:guid}/expire", async (ITranslationQuizGameService gameService, Guid roundId, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.ExpireRoundAsync(roundId, cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        return Results.NotFound(new ProblemDetails { Title = "Round not found", Detail = ex.Message });
                    }
                })
                .WithName("TranslationQuizExpireRound")
                .ExcludeFromDescription();

            await app.RunAsync();
        }
    }
}
