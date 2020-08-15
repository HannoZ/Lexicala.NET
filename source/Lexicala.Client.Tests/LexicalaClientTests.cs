using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using Moq.Protected;
using Shouldly;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lexicala.NET.Client.Tests
{
    [TestClass]
    public class LexicalaClientTests
    {
        // NOTE - to mock the HttpClient you need to mock the HttpMessageHandler that is used by the client.
        // this requires to mock the internal protected 'SendAsync' method, which can by done with Moq using the not-so-common Protect namespace

        private Mock<HttpMessageHandler> _handlerMock;
        private LexicalaClient _client;

        [TestInitialize]
        public void Initialize()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://www.tempuri.org")
            };

            var mocker = new AutoMocker(MockBehavior.Loose);
            mocker.Use(httpClient);

            _client = mocker.CreateInstance<LexicalaClient>();
        }

        [TestMethod]
        public async Task LexicalaClient_TestAsync()
        {
            string response = await LoadResponseFromFile("Test_Api_is_up.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.TestAsync();

            // ASSERT
            result.Message.ShouldBe("API is up");
        }

        [TestMethod]
        public async Task LexicalaClient_MeAsync()
        {
            string response = await LoadResponseFromFile("me.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.MeAsync();

            // ASSERT
            result.Email.ShouldBe("foo@bar.com");
            result.Permissions.Activation.Activated.ShouldBeTrue();
        }

        [TestMethod]
        public async Task LexicalaClient_LanguagesAsync()
        {
            string response = await LoadResponseFromFile("languages.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.LanguagesAsync();

            // ASSERT
            result.LanguageNames.ShouldNotBeEmpty();
            result.Resources.Global.SourceLanguages.ShouldNotBeEmpty();
            result.Resources.Password.SourceLanguages.ShouldNotBeEmpty();
            result.Resources.Random.SourceLanguages.ShouldNotBeEmpty();

        }

        [TestMethod]
        public async Task LexicalaClient_BasicSearch_Es_Hacer()
        {
            string response = await LoadResponseFromFile("Search_Es_Hacer.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.BasicSearchAsync("searchText", "es");

            result.Results.Length.ShouldBe(3);
        }

        [TestMethod]
        public async Task LexicalaClient_GetEntry_En_Place()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.GetEntryAsync("EN_DE00009032");

            result.Id.ShouldBe("EN_DE00009032");
            result.Senses.Length.ShouldBe(12);
        }

        private static HttpResponseMessage SetupOkResponseMessage(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
        }

        private static Task<string> LoadResponseFromFile(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var resourceStream = asm.GetManifestResourceStream($"Lexicala.NET.Client.Tests.Resources.{fileName}");

            if (resourceStream != null)
            {
                using var reader = new StreamReader(resourceStream);
                return reader.ReadToEndAsync();
            }

            return Task.FromResult(string.Empty);
        }
    }
}
