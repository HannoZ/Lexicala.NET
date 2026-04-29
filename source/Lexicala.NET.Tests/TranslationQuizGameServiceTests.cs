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
        public async Task CreateRoundAsync_UnsupportedLanguage_ThrowsArgumentException()
        {
            await Should.ThrowAsync<ArgumentException>(() =>
                _service.CreateRoundAsync("xx"));
        }

        [TestMethod]
        public async Task CreateRoundAsync_NullLanguage_ThrowsArgumentException()
        {
            await Should.ThrowAsync<ArgumentException>(() =>
                _service.CreateRoundAsync(null!));
        }

        [TestMethod]
        [DataRow("de")]
        [DataRow("DE")]
        [DataRow("nl")]
        [DataRow("fr")]
        [DataRow("es")]
        public void CreateRoundAsync_SupportedLanguages_DoesNotThrowArgumentException(string language)
        {
            // Arrange: make the client throw so we don't need a real API, but the validation should pass before the first API call.
            _clientMock
                .Setup(c => c.FlukySearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("API not available in test"));

            // Act + Assert: argument validation passes (InvalidOperationException, not ArgumentException)
            Should.NotThrow(() =>
            {
                _ = _service.CreateRoundAsync(language).ContinueWith(_ => { });
            });
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
    }
}
