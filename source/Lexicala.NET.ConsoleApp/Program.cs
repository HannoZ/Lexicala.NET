using Lexicala.NET;
using Lexicala.NET.Parsing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lexicala.NET.Request;
using System.Threading;
using System.Threading.Tasks;

namespace Lexicala.NET.ConsoleApp
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName);
            });
            builder.Services.AddLogging(cfg => cfg.AddConsole());

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapGet("/health", () => Results.Ok("OK")).WithName("Health");

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

            await app.RunAsync();
        }
    }
}
