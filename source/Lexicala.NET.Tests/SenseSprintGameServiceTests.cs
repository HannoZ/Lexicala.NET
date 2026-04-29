using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lexicala.NET.Demo.Api.Game;
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
    }
}
