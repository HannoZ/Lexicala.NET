using Lexicala.NET.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using System.Text.Json;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Parsing;
using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Entries.JsonConverters;
using Lexicala.NET.Response.Languages;
using Lexicala.NET.Response.Search;

namespace Lexicala.NET.Parser.Tests
{
    [TestClass]
    public class LexicalaSearchParserTests
    {
        private LexicalaSearchParser _lexicalaSearchParser;
        private AutoMocker _mocker;

        [TestInitialize]
        public void Initialize()
        {
            _mocker = new AutoMocker(MockBehavior.Loose);
            _lexicalaSearchParser = _mocker.CreateInstance<LexicalaSearchParser>();

            var languagesResponse = new LanguagesResponse()
            {
                Resources = new Resources()
                {
                    Global = new Resource()
                    {
                        SourceLanguages = new[]
                        {
                            "en", "es"
                        }
                    }
                }
            };

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.LanguagesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(languagesResponse);

            _mocker.GetMock<IMemoryCache>()
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(_mocker.GetMock<ICacheEntry>().Object);
        }

        [TestMethod]
        public async Task LexicalaDetailsLoader_SearchAsync_1()
        {
            string searchResult = await LoadResponseFromFile("Search_es_blando_analyzed.json");
            string entry1 = await LoadResponseFromFile("ES_DE00008087.json");
            string entry2 = await LoadResponseFromFile("ES_DE00008088.json");

            var apiResult = JsonSerializer.Deserialize<SearchResponse>(searchResult, SearchResponseJsonConverter.Settings);
            apiResult.Metadata = new ResponseMetadata();
            var entryResult1 = JsonSerializer.Deserialize<Entry>(entry1, EntryResponseJsonConverter.Settings);
            entryResult1.Metadata = new ResponseMetadata();
            var entryResult2 = JsonSerializer.Deserialize<Entry>(entry2, EntryResponseJsonConverter.Settings);
            entryResult2.Metadata = new ResponseMetadata();

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.BasicSearchAsync("test", "es", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiResult);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008088", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entryResult2);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008087", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entryResult1);


            // ACT
            var result = await _lexicalaSearchParser.SearchAsync("test", "es");

            // ASSERT
            result.Summary("nl").ShouldBe("blandir: zwaaien | blando/blanda: zacht, toegeeflijk, laf");
            result.Results.SelectMany(r => r.Stems).ShouldNotBeEmpty();
            result.Results.ShouldAllBe(r => !r.Pos.StartsWith("System.String"));
        }

        [TestMethod]
        public async Task LexicalSearchParser_Parse_ES_DE00019850()
        {
            string searchResult = await LoadResponseFromFile("Search_es_sin embargo.json");
            string entry1 = await LoadResponseFromFile("ES_DE00019850.json");

            var apiResult = JsonSerializer.Deserialize<SearchResponse>(searchResult, SearchResponseJsonConverter.Settings);
            apiResult.Metadata = new ResponseMetadata();
            var entryResult1 = JsonSerializer.Deserialize<Entry>(entry1, EntryResponseJsonConverter.Settings);
            entryResult1.Metadata = new ResponseMetadata();

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.BasicSearchAsync("test", "es", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiResult);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Entry());
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00019850", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entryResult1);


            var result = await _lexicalaSearchParser.SearchAsync("test", "es");

            var comps = result.Results.SelectMany(r => r.Senses).SelectMany(s => s.CompositionalPhrases).ToList();
            comps.ShouldNotBeEmpty();
            foreach (var comp in comps)
            {
                comp.Definition.ShouldNotBeNull();
                comp.Examples.ShouldNotBeEmpty();
                comp.Text.ShouldNotBeNull();
                comp.Translations.ShouldNotBeEmpty();
            }
        }

        [TestMethod]
        public async Task LexicalaSearchParser_SearchAsync_MultipleTargetLanguages()
        {
            string searchResult = await LoadResponseFromFile("Search_es_blando_analyzed.json");
            string entry1 = await LoadResponseFromFile("ES_DE00008087.json");

            var apiResult = JsonSerializer.Deserialize<SearchResponse>(searchResult, SearchResponseJsonConverter.Settings);
            apiResult.Metadata = new ResponseMetadata();
            var entryResult1 = JsonSerializer.Deserialize<Entry>(entry1, EntryResponseJsonConverter.Settings);
            entryResult1.Metadata = new ResponseMetadata();

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.BasicSearchAsync("test", "es", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiResult);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entryResult1);

            // ACT
            var result = await _lexicalaSearchParser.SearchAsync("test", "es", "en", "nl");

            // ASSERT
            result.ShouldNotBeNull();
            result.SearchText.ShouldBe("test");
            result.Results.ShouldNotBeEmpty();
        }

        [TestMethod]
        public async Task LexicalaSearchParser_SearchAsync_InvalidSourceLanguage_ThrowsException()
        {
            // ACT & ASSERT
            await Should.ThrowAsync<ArgumentException>(async () =>
                await _lexicalaSearchParser.SearchAsync("test", "invalid"));
        }

        [TestMethod]
        public async Task LexicalaSearchParser_SearchAsync_EmptySearchText_ThrowsException()
        {
            // ACT & ASSERT
            await Should.ThrowAsync<ArgumentException>(async () =>
                await _lexicalaSearchParser.SearchAsync("", "es"));
        }

        [TestMethod]
        public async Task LexicalaSearchParser_SearchAsync_NullSearchText_ThrowsException()
        {
            // ACT & ASSERT
            await Should.ThrowAsync<ArgumentException>(async () =>
                await _lexicalaSearchParser.SearchAsync((string)null, "es"));
        }

        [TestMethod]
        public async Task LexicalaSearchParser_AdvancedSearchAsync_InvalidLanguage_ThrowsException()
        {
            var searchRequest = new AdvancedSearchRequest
            {
                Language = "invalid",
                SearchText = "test"
            };

            // ACT & ASSERT
            await Should.ThrowAsync<ArgumentException>(async () =>
                await _lexicalaSearchParser.SearchAsync(searchRequest));
        }

        [TestMethod]
        public async Task LexicalaSearchParser_AdvancedSearchAsync_EmptySearchText_ThrowsException()
        {
            var searchRequest = new AdvancedSearchRequest
            {
                Language = "es",
                SearchText = ""
            };

            // ACT & ASSERT
            await Should.ThrowAsync<ArgumentException>(async () =>
                await _lexicalaSearchParser.SearchAsync(searchRequest));
        }

        [TestMethod]
        public async Task LexicalaSearchParser_AdvancedSearchAsync_NullRequest_ThrowsException()
        {
            // ACT & ASSERT
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await _lexicalaSearchParser.SearchAsync((AdvancedSearchRequest)null));
        }

        [TestMethod]
        public async Task LexicalaSearchParser_GetEntryAsync_ValidEntryId()
        {
            string entry1 = await LoadResponseFromFile("ES_DE00008087.json");
            var entryResult1 = JsonSerializer.Deserialize<Entry>(entry1, EntryResponseJsonConverter.Settings);
            entryResult1.Metadata = new ResponseMetadata();

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008087", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entryResult1);

            // ACT
            var result = await _lexicalaSearchParser.GetEntryAsync("ES_DE00008087");

            // ASSERT
            result.ShouldNotBeNull();
            result.Id.ShouldBe("ES_DE00008087");
        }

        [TestMethod]
        public async Task LexicalaSearchParser_GetEntryAsync_WithTargetLanguages()
        {
            string entry1 = await LoadResponseFromFile("ES_DE00008087.json");
            var entryResult1 = JsonSerializer.Deserialize<Entry>(entry1, EntryResponseJsonConverter.Settings);
            entryResult1.Metadata = new ResponseMetadata();

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008087", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entryResult1);

            // ACT
            var result = await _lexicalaSearchParser.GetEntryAsync("ES_DE00008087", "en", "nl");

            // ASSERT
            result.ShouldNotBeNull();
            result.Id.ShouldBe("ES_DE00008087");
        }

        private static Task<string> LoadResponseFromFile(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            var x = asm.GetManifestResourceNames();
            using var resourceStream = asm.GetManifestResourceStream($"Lexicala.NET.Tests.Resources.{fileName}");

            if (resourceStream != null)
            {
                using var reader = new StreamReader(resourceStream);
                return reader.ReadToEndAsync();
            }

            return Task.FromResult(string.Empty);
        }
    }
}

