using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
    public class LexicalaClientBasicSearchTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_BasicSearch_InvalidLanguageCode_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.BasicSearchAsync("searchText", "ess"));
        }

        [TestMethod]
        public async Task LexicalaClient_BasicSearch_EmptySearchText_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.BasicSearchAsync("", "es"));
        }

        [TestMethod]
        public async Task LexicalaClient_BasicSearch_Es_Hacer()
        {
            string response = await LoadResponseFromFile("Search_Es_Hacer.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.BasicSearchAsync("searchText", "es");
            result.Results.Length.ShouldBe(3);
        }

        [TestMethod]
        public async Task LexicalaClient_Search_ETag()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            await Client.BasicSearchAsync("text", "xx", "W/\"abc-OfxtVSoa\"");

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Headers.Contains("If-None-Match")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_Search_Metadata()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            var responseMessage = SetupOkResponseMessage(response);
            responseMessage.Headers.ETag = new EntityTagHeaderValue("\"abc-OfxtVSoa\"");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRequestsRemaining, "100");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRequestsLimit, "1000");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitReset, "12345");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var result = await Client.BasicSearchAsync("text", "xx");

            result.Metadata.ETag.ShouldBe("\"abc-OfxtVSoa\"");
            result.Metadata.RateLimits.LimitRemaining.ShouldBe(100);
            result.Metadata.RateLimits.Limit.ShouldBe(1000);
            result.Metadata.RateLimits.Reset.ShouldBe(12345);
        }
    }
}
