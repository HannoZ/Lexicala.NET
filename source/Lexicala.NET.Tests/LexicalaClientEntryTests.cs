using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Lexicala.NET.Response;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Shouldly;

namespace Lexicala.NET.Client.Tests
{
    [TestClass]
    public class LexicalaClientEntryTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_GetEntry_En_Place()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.GetEntryAsync("EN_DE00009032");
            result.Id.ShouldBe("EN_DE00009032");
            result.Senses.Length.ShouldBe(12);
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_EN_DE00009032()
        {
            await AssertEntryDeserializes("Entry_EN_DE00009032.json", "EN_DE00009032");
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_ES_DE00008087()
        {
            await AssertEntryDeserializes("Entry_ES_DE00008087.json", "ES_DE00008087");
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_ES_DE00008089()
        {
            await AssertEntryDeserializes("Entry_ES_DE00008088.json", "ES_DE00008088");
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_NL_DE00006941()
        {
            await AssertEntryDeserializes("Entry_NL_DE00006941.json", "NL_DE00006941");
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_PW00012363()
        {
            await AssertEntryDeserializes("Entry_PW00012363.json", "PW00012363");
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_RDE00032314_1()
        {
            await AssertEntryDeserializes("Entry_RDE00032314_1.json", "RDE00032314_1");
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_ES_DE00010530()
        {
            await AssertEntryDeserializes("ES_DE00010530.json", "ES_DE00010530");
        }

        [TestMethod]
        public async Task LexicalaClient_GetEntry_ETag()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            await Client.GetEntryAsync("EN_DE00009032", "W/\"abc-OfxtVSoa\"");

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Headers.Contains("If-None-Match")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_GetEntry_Metadata()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            var responseMessage = SetupOkResponseMessage(response);
            responseMessage.Headers.ETag = new EntityTagHeaderValue("\"abc-OfxtVSoa\"");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRequestsRemaining, "75");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRequestsLimit, "100");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitReset, "12345");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRapidFreePlanHardLimitLimit, "5000");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRapidFreePlanHardLimitRemaining, "1000");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRapidFreePlanHardLimitReset, "12345");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var result = await Client.GetEntryAsync("EN_DE00009032");

            result.Metadata.ETag.ShouldBe("\"abc-OfxtVSoa\"");
            result.Metadata.RateLimits.LimitRemaining.ShouldBe(75);
            result.Metadata.RateLimits.Limit.ShouldBe(100);
            result.Metadata.RateLimits.Reset.ShouldBe(12345);
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_ES_DE00019850()
        {
            await AssertEntryDeserializes("ES_DE00019850.json", "ES_DE00019850");
        }

        [TestMethod]
        public async Task LexicalaClient_SearchEntries_Basic_IncludesSearchEntriesEndpoint()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage($"[{response}]"));

            var result = await Client.SearchEntriesAsync("text", "xx");

            result.ShouldNotBeNull();
            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/search-entries?language=xx&text=text"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearchEntries_IncludesSearchEntriesEndpoint()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage($"[{response}]"));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Synonyms = true
            };

            var result = await Client.AdvancedSearchEntriesAsync(searchRequest);

            result.ShouldNotBeNull();
            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/search-entries?language=xx&text=text&source=global&synonyms=true"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_GetEntryAsync_EncodesEntryIdInPath()
        {
            const string response = "{\"id\":\"id\",\"headword\":[],\"senses\":[],\"related_entries\":[]}";

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            await Client.GetEntryAsync("EN_DE/unsafe");

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/entries/EN_DE%2Funsafe"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_GetSenseAsync_EncodesSenseIdInPath()
        {
            const string response = "{\"id\":\"sense-id\"}";

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            await Client.GetSenseAsync("EN_SE/unsafe");

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/senses/EN_SE%2Funsafe"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public void Headword_PartOfSpeeches_ReturnsEmpty_WhenPosIsAbsent()
        {
            var headword = new Lexicala.NET.Response.Entries.Headword();

            headword.PartOfSpeeches.ShouldBeEmpty();
        }

        private async Task AssertEntryDeserializes(string resourceFile, string entryId)
        {
            string response = await LoadResponseFromFile(resourceFile);

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.GetEntryAsync(entryId);
            result.ShouldNotBeNull();
        }
    }
}
