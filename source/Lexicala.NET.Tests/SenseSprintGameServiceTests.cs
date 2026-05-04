using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lexicala.NET.Demo.Api.Game;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Lexicala.NET.Client.Tests
{
    [TestClass]
    public class SenseSprintGameServiceTests
    {
        private IMemoryCache _cache;
        private Mock<ILexicalaClient> _clientMock;
        private Mock<ILogger<SenseSprintGameService>> _loggerMock;
        private SenseSprintGameService _service;

        [TestInitialize]
        public void Initialize()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _clientMock = new Mock<ILexicalaClient>();
            _loggerMock = new Mock<ILogger<SenseSprintGameService>>();
            _service = new SenseSprintGameService(_clientMock.Object, _cache, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cache.Dispose();
        }

        [TestMethod]
        public async Task SubmitGuessAsync_NullGuess_ThrowsArgumentException()
        {
            await Should.ThrowAsync<ArgumentException>(() =>
                _service.SubmitGuessAsync(Guid.NewGuid(), null!));
        }

        [TestMethod]
        public async Task SubmitGuessAsync_WhitespaceGuess_ThrowsArgumentException()
        {
            await Should.ThrowAsync<ArgumentException>(() =>
                _service.SubmitGuessAsync(Guid.NewGuid(), "   "));
        }

        [TestMethod]
        public async Task SubmitGuessAsync_RoundNotFound_ThrowsKeyNotFoundException()
        {
            var unknownId = Guid.NewGuid();

            await Should.ThrowAsync<KeyNotFoundException>(() =>
                _service.SubmitGuessAsync(unknownId, "house"));
        }

        [TestMethod]
        public async Task RevealNextClueAsync_RoundNotFound_ThrowsKeyNotFoundException()
        {
            var unknownId = Guid.NewGuid();

            await Should.ThrowAsync<KeyNotFoundException>(() =>
                _service.RevealNextClueAsync(unknownId));
        }

        [TestMethod]
        public async Task GiveUpAsync_RoundNotFound_ThrowsKeyNotFoundException()
        {
            var unknownId = Guid.NewGuid();

            await Should.ThrowAsync<KeyNotFoundException>(() =>
                _service.GiveUpAsync(unknownId));
        }

        [TestMethod]
        public async Task CreateRoundAsync_ReturnsRequestedLanguage()
        {
            _clientMock
                .Setup(c => c.FlukySearchAsync(It.IsAny<string>(), "de", It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new SearchResponse
                {
                    Results = [new Result { Id = "DE0001" }]
                });

            _clientMock
                .Setup(c => c.GetEntryAsync("DE0001", null, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new Entry
                {
                    Id = "DE0001",
                    HeadwordObject = new Lexicala.NET.Response.Entries.Headword
                    {
                        Text = "Haus",
                        Pos = "noun"
                    },
                    Senses =
                    [
                        new Lexicala.NET.Response.Entries.Sense
                        {
                            Definition = "a building for human habitation",
                            Synonyms = ["Gebaude"],
                            Examples = [new Example { Text = "Das Haus ist gross." }]
                        }
                    ]
                });

            var round = await _service.CreateRoundAsync("de");

            round.Language.ShouldBe("de");
            round.Clue.ShouldNotBeNullOrWhiteSpace();
        }
    }
}
