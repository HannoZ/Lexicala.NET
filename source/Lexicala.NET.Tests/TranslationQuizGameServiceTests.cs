using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Demo.Api.Game;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Languages;
using Lexicala.NET.Response.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Lexicala.NET.Client.Tests
{
    [TestClass]
    public class TranslationQuizGameServiceTests
    {
        private IMemoryCache _cache;
        private Mock<ILexicalaClient> _clientMock;
        private Mock<ILogger<TranslationQuizGameService>> _loggerMock;
        private TranslationQuizGameService _service;

        [TestInitialize]
        public void Initialize()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _clientMock = new Mock<ILexicalaClient>();
            _loggerMock = new Mock<ILogger<TranslationQuizGameService>>();
            _service = new TranslationQuizGameService(_clientMock.Object, _cache, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.Dispose();
        }

        [TestMethod]
        public async Task CreateRoundAsync_NullLanguage_FetchesLanguagesFromApi()
        {
            // Arrange: language API returns a list; FlukySearch throws so round generation fails fast
            _clientMock
                .Setup(c => c.LanguagesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguagesResponse
                {
                    LanguageNames = new Dictionary<string, string> { { "de", "German" }, { "nl", "Dutch" } },
                    Resources = new Resources { Global = new Resource { SourceLanguages = ["de", "nl"] } }
                });
            _clientMock
                .Setup(c => c.FlukySearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("API not available in test"));

            // Act + Assert: null is not ArgumentException; execution reaches the mocked client
            await Should.ThrowAsync<InvalidOperationException>(() =>
                _service.CreateRoundAsync(null));

            _clientMock.Verify(c => c.LanguagesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [DataRow("de")]
        [DataRow("DE")]
        [DataRow("nl")]
        [DataRow("fr")]
        [DataRow("es")]
        [DataRow("ja")]
        [DataRow("zh")]
        [DataRow("xx")]
        public async Task CreateRoundAsync_AnyLanguage_AcceptsWithoutArgumentException(string language)
        {
            // Arrange: make the client throw after validation so we can verify it reached the API call
            _clientMock
                .Setup(c => c.LanguagesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguagesResponse
                {
                    LanguageNames = new Dictionary<string, string>(),
                    Resources = new Resources { Global = new Resource { SourceLanguages = ["de", "nl"] } }
                });
            _clientMock
                .Setup(c => c.FlukySearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("API not available in test"));

            // Act + Assert: any language passes validation; execution reaches the mocked API call
            await Should.ThrowAsync<InvalidOperationException>(() =>
                _service.CreateRoundAsync(language));
        }

        [TestMethod]
        public async Task GetTargetLanguagesAsync_CachesResult()
        {
            _clientMock
                .Setup(c => c.LanguagesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguagesResponse
                {
                    LanguageNames = new Dictionary<string, string>(),
                    Resources = new Resources { Global = new Resource { SourceLanguages = ["de", "nl", "fr"] } }
                });

            // Call twice
            var first = await _service.GetTargetLanguagesAsync(CancellationToken.None);
            var second = await _service.GetTargetLanguagesAsync(CancellationToken.None);

            first.ShouldBe(second);
            _clientMock.Verify(c => c.LanguagesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetTargetLanguagesAsync_ExcludesEnglish()
        {
            _clientMock
                .Setup(c => c.LanguagesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguagesResponse
                {
                    LanguageNames = new Dictionary<string, string>(),
                    Resources = new Resources { Global = new Resource { SourceLanguages = ["en", "de", "nl"] } }
                });

            var languages = await _service.GetTargetLanguagesAsync(CancellationToken.None);

            languages.ShouldNotContain("en");
            languages.ShouldContain("de");
            languages.ShouldContain("nl");
        }

        [TestMethod]
        public async Task GetTargetLanguagesAsync_PrefersTargetLanguagesOverSourceLanguages()
        {
            _clientMock
                .Setup(c => c.LanguagesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LanguagesResponse
                {
                    LanguageNames = new Dictionary<string, string>(),
                    Resources = new Resources
                    {
                        Global = new Resource
                        {
                            SourceLanguages = ["en", "de", "nl"],
                            TargetLanguages = ["de", "fr"]
                        }
                    }
                });

            var languages = await _service.GetTargetLanguagesAsync(CancellationToken.None);

            languages.ShouldContain("de");
            languages.ShouldContain("fr");
            languages.ShouldNotContain("nl"); // only in source, not target
        }

        [TestMethod]
        public async Task GetTargetLanguagesAsync_FallsBackOnApiError()
        {
            _clientMock
                .Setup(c => c.LanguagesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("API error"));

            var languages = await _service.GetTargetLanguagesAsync(CancellationToken.None);

            languages.ShouldNotBeEmpty();
        }

        [TestMethod]
        public async Task CreateRoundAsync_ReusesCachedDistractors_ForSameTargetLanguage()
        {
            _clientMock
                .Setup(c => c.GetEntryAsync("EN0001", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Entry
                {
                    Id = "EN0001",
                    HeadwordObject = new Lexicala.NET.Response.Entries.Headword { Text = "house" },
                    Senses =
                    [
                        new Lexicala.NET.Response.Entries.Sense
                        {
                            Translations = new Dictionary<string, TranslationObject>
                            {
                                ["de"] = new Translation { Text = "Haus" }
                            }
                        }
                    ]
                });

            _clientMock
                .Setup(c => c.FlukySearchAsync(It.IsAny<string>(), "en", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SearchResponse
                {
                    Results = [new Result { Id = "EN0001" }]
                });

            var distractorResponses = new Queue<SearchResponse>(
            [
                BuildDistractorResponse("Auto"),
                BuildDistractorResponse("Baum"),
                BuildDistractorResponse("Buch")
            ]);

            _clientMock
                .Setup(c => c.FlukySearchAsync(It.IsAny<string>(), "de", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => distractorResponses.Dequeue());

            var firstRound = await _service.CreateRoundAsync("de", CancellationToken.None);
            var secondRound = await _service.CreateRoundAsync("de", CancellationToken.None);

            firstRound.Choices.Length.ShouldBe(4);
            secondRound.Choices.Length.ShouldBe(4);
            _clientMock.Verify(c => c.FlukySearchAsync(It.IsAny<string>(), "de", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task SubmitAnswerAsync_RoundNotFound_ThrowsKeyNotFoundException()
        {
            var unknownId = Guid.NewGuid();

            await Should.ThrowAsync<KeyNotFoundException>(() =>
                _service.SubmitAnswerAsync(unknownId, "someChoice"));
        }

        [TestMethod]
        public async Task SubmitAnswerAsync_NullChoice_ThrowsArgumentException()
        {
            await Should.ThrowAsync<ArgumentException>(() =>
                _service.SubmitAnswerAsync(Guid.NewGuid(), null!));
        }

        [TestMethod]
        public async Task SubmitAnswerAsync_EmptyChoice_ThrowsArgumentException()
        {
            await Should.ThrowAsync<ArgumentException>(() =>
                _service.SubmitAnswerAsync(Guid.NewGuid(), string.Empty));
        }

        [TestMethod]
        public async Task ExpireRoundAsync_RoundNotFound_ThrowsKeyNotFoundException()
        {
            var unknownId = Guid.NewGuid();

            await Should.ThrowAsync<KeyNotFoundException>(() =>
                _service.ExpireRoundAsync(unknownId));
        }

        private static SearchResponse BuildDistractorResponse(string headword)
        {
            return new SearchResponse
            {
                Results =
                [
                    new Result
                    {
                        Id = headword,
                        Headword = new Lexicala.NET.Response.Search.Headword { Text = headword }
                    }
                ]
            };
        }
    }
}
