using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Response;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Shouldly;

namespace Lexicala.NET.Client.Tests
{
    [TestClass]
    public class LexicalaClientHealthAndLanguagesTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_TestAsync()
        {
            string response = await LoadResponseFromFile("Test_Api_is_up.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.TestAsync();
            result.Message.ShouldBe("API is up");
        }

        [TestMethod]
        public async Task LexicalaClient_TestAsync_ApiError_ThrowsLexicalaApiException()
        {
            const string response = "{\"message\":\"Bad request\"}";

            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(response)
            };
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRequestsRemaining, "1");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRequestsLimit, "10");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitReset, "100");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var exception = await Should.ThrowAsync<LexicalaApiException>(async () => await Client.TestAsync());
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            exception.Message.ShouldBe("Bad request");
            exception.Metadata.RateLimits.LimitRemaining.ShouldBe(1);
            exception.Metadata.RateLimits.Limit.ShouldBe(10);
            exception.Metadata.RateLimits.Reset.ShouldBe(100);
        }

        [TestMethod]
        public async Task LexicalaClient_LanguagesAsync()
        {
            string response = await LoadResponseFromFile("languages.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.LanguagesAsync();
            result.LanguageNames.ShouldNotBeEmpty();
            result.Resources.Global.SourceLanguages.ShouldNotBeEmpty();
            result.Resources.Password.SourceLanguages.ShouldNotBeEmpty();
            result.Resources.Random.SourceLanguages.ShouldNotBeEmpty();
        }
    }
}
