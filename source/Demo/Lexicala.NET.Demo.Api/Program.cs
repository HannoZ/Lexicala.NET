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
                options.CustomSchemaIds(type => type.FullName);
            });
            builder.Services.AddLogging(cfg => cfg.AddConsole());

            var app = builder.Build();

            app.UseCors("ReactDev");
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapGet("/test", async (ILexicalaClient client, CancellationToken cancellationToken) =>
                await client.TestAsync(cancellationToken))
                .WithName("Test");

            app.MapGet("/languages", async (ILexicalaClient client, CancellationToken cancellationToken) =>
                await client.LanguagesAsync(cancellationToken))
                .WithName("Languages");

            app.MapGet("/search", async (ILexicalaClient client, string text, string language, string? etag, CancellationToken cancellationToken) =>
                await client.BasicSearchAsync(text, language, etag, cancellationToken))
                .WithName("BasicSearch");

            app.MapGet("/search-entries", async (ILexicalaClient client, string text, string language, string? etag, CancellationToken cancellationToken) =>
                await client.SearchEntriesAsync(text, language, etag, cancellationToken))
                .WithName("SearchEntries");

            app.MapGet("/search-rdf", async (ILexicalaClient client, string text, string language, string? etag, CancellationToken cancellationToken) =>
                Results.Text(await client.SearchRdfAsync(text, language, etag, cancellationToken), "application/ld+json"))
                .WithName("SearchRdf");

            app.MapGet("/entry/{entryId}", async (ILexicalaClient client, string entryId, string? etag, CancellationToken cancellationToken) =>
                await client.GetEntryAsync(entryId, etag, cancellationToken))
                .WithName("GetEntry");

            app.MapGet("/sense/{senseId}", async (ILexicalaClient client, string senseId, string? etag, CancellationToken cancellationToken) =>
                await client.GetSenseAsync(senseId, etag, cancellationToken))
                .WithName("GetSense");

            app.MapGet("/rdf/{entryId}", async (ILexicalaClient client, string entryId, string? etag, CancellationToken cancellationToken) =>
                Results.Text(await client.GetRdfAsync(entryId, etag, cancellationToken), "application/ld+json"))
                .WithName("GetRdf");

            app.MapPost("/search-advanced", async (ILexicalaClient client, AdvancedSearchRequest request, CancellationToken cancellationToken) =>
                await client.AdvancedSearchAsync(request, cancellationToken))
                .WithName("AdvancedSearch");

            app.MapPost("/search-entries-advanced", async (ILexicalaClient client, AdvancedSearchRequest request, CancellationToken cancellationToken) =>
                await client.AdvancedSearchEntriesAsync(request, cancellationToken))
                .WithName("AdvancedSearchEntries");

            app.MapPost("/search-rdf-advanced", async (ILexicalaClient client, AdvancedSearchRequest request, CancellationToken cancellationToken) =>
                Results.Text(await client.AdvancedSearchRdfAsync(request, cancellationToken), "application/ld+json"))
                .WithName("AdvancedSearchRdf");

            app.MapGet("/search-definitions", async (ILexicalaClient client, string text, string? language, string? etag, CancellationToken cancellationToken) =>
                await client.SearchDefinitionsAsync(text, language, etag, cancellationToken))
                .WithName("SearchDefinitions");

            app.MapGet("/fluky-search", async (ILexicalaClient client, string? source, string? language, string? etag, CancellationToken cancellationToken) =>
                await client.FlukySearchAsync(source ?? "global", language, etag, cancellationToken))
                .WithName("FlukySearch");

            app.MapPost("/game/sense-sprint/rounds", async (ISenseSprintGameService gameService, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        var response = await gameService.CreateRoundAsync(cancellationToken);
                        return Results.Ok(response);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
                    }
                })
                .WithName("SenseSprintCreateRound");

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
                .WithName("SenseSprintNextClue");

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
                .WithName("SenseSprintSubmitGuess");

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
                .WithName("SenseSprintGiveUp");

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
                .WithName("TranslationQuizCreateRound");

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
                .WithName("TranslationQuizSubmitAnswer");

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
                .WithName("TranslationQuizExpireRound");

            await app.RunAsync();
        }
    }
}
