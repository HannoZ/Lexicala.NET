using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lexicala.NET.Client.Tests
{
    public abstract class LexicalaClientTestBase
    {
        protected Mock<HttpMessageHandler> HandlerMock;
        protected LexicalaClient Client;

        [TestInitialize]
        public void Initialize()
        {
            HandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(HandlerMock.Object)
            {
                BaseAddress = new Uri("http://www.tempuri.org")
            };

            Client = CreateClient(httpClient, useLiteEndpoints: false);
        }

        protected void InitializeClient(bool useLiteEndpoints)
        {
            var httpClient = new HttpClient(HandlerMock.Object)
            {
                BaseAddress = new Uri("http://www.tempuri.org")
            };

            Client = CreateClient(httpClient, useLiteEndpoints);
        }

        protected LexicalaClient CreateClient(HttpClient httpClient, bool useLiteEndpoints)
        {
            var mocker = new AutoMocker(MockBehavior.Loose);
            mocker.Use(httpClient);
            var config = new LexicalaConfig("test-key", useLiteEndpoints);
            mocker.Use<IOptions<LexicalaConfig>>(Options.Create(config));
            return mocker.CreateInstance<LexicalaClient>();
        }

        protected static HttpResponseMessage SetupOkResponseMessage(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
        }

        protected static async Task<string> LoadResponseFromFile(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var resourceStream = asm.GetManifestResourceStream($"Lexicala.NET.Tests.Resources.{fileName}");

            if (resourceStream != null)
            {
                using var reader = new StreamReader(resourceStream);
                return await reader.ReadToEndAsync();
            }

            return string.Empty;
        }
    }
}
