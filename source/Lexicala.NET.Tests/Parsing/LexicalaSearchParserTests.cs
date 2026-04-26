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

        [TestMethod]
        public async Task LexicalaSearchParser_SearchAsync_WithTargetLanguages_FiltersTranslations()
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
            result.Results.ShouldNotBeEmpty();

            // Verify that senses only contain translations for the specified target languages
            foreach (var resultEntry in result.Results)
            {
                foreach (var sense in resultEntry.Senses)
                {
                    // All translations should only be in "en" or "nl"
                    foreach (var translation in sense.Translations)
                    {
                        translation.Language.ShouldBeOneOf("en", "nl");
                    }

                    // Verify examples also respect the target language filter
                    foreach (var example in sense.Examples)
                    {
                        foreach (var translation in example.Translations)
                        {
                            translation.Language.ShouldBeOneOf("en", "nl");
                        }
                    }

                    // Verify compositional phrases also respect the target language filter
                    foreach (var compPhrase in sense.CompositionalPhrases)
                    {
                        foreach (var translation in compPhrase.Translations)
                        {
                            translation.Language.ShouldBeOneOf("en", "nl");
                        }

                        foreach (var example in compPhrase.Examples)
                        {
                            foreach (var translation in example.Translations)
                            {
                                translation.Language.ShouldBeOneOf("en", "nl");
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task LexicalaSearchParser_SearchAsync_NoTargetLanguages_ReturnsAllTranslations()
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
            var result = await _lexicalaSearchParser.SearchAsync("test", "es");

            // ASSERT
            result.ShouldNotBeNull();
            result.Results.ShouldNotBeEmpty();

            // Verify that senses contain translations in all available languages
            var allLanguages = result.Results
                .SelectMany(r => r.Senses)
                .SelectMany(s => s.Translations)
                .Select(t => t.Language)
                .Distinct()
                .ToList();

            // Should have multiple languages (br, en, ja, nl, no, sv, etc.)
            allLanguages.Count.ShouldBeGreaterThan(1);
        }

        [TestMethod]
        public async Task LexicalaSearchParser_SearchAsync_AdvancedSearch_WithTargetLanguages()
        {
            string searchResult = await LoadResponseFromFile("Search_es_blando_analyzed.json");
            string entry1 = await LoadResponseFromFile("ES_DE00008087.json");

            var apiResult = JsonSerializer.Deserialize<SearchResponse>(searchResult, SearchResponseJsonConverter.Settings);
            apiResult.Metadata = new ResponseMetadata();
            var entryResult1 = JsonSerializer.Deserialize<Entry>(entry1, EntryResponseJsonConverter.Settings);
            entryResult1.Metadata = new ResponseMetadata();

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.AdvancedSearchAsync(It.IsAny<AdvancedSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiResult);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entryResult1);

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "es",
                SearchText = "test"
            };

            // ACT
            var result = await _lexicalaSearchParser.SearchAsync(searchRequest, "en", "fr");

            // ASSERT
            result.ShouldNotBeNull();
            result.Results.ShouldNotBeEmpty();

            // Verify that only "en" and "fr" translations are returned
            foreach (var resultEntry in result.Results)
            {
                foreach (var sense in resultEntry.Senses)
                {
                    foreach (var translation in sense.Translations)
                    {
                        translation.Language.ShouldBeOneOf("en", "fr");
                    }
                }
            }
        }

        [TestMethod]
        public async Task LexicalaSearchParser_GetEntryAsync_WithTargetLanguages_FiltersTranslations()
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

            // Verify translations are filtered to only "en" and "nl"
            foreach (var sense in result.Senses)
            {
                foreach (var translation in sense.Translations)
                {
                    translation.Language.ShouldBeOneOf("en", "nl");
                }
            }
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

