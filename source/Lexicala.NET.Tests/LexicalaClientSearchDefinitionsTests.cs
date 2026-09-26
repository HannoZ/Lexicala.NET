using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Shouldly;

namespace Lexicala.NET.Client.Tests
{
    [TestClass]
    public class LexicalaClientSearchDefinitionsTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_SearchDefinitions_EmptySearchText_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.SearchDefinitionsAsync(""));
        }

        [TestMethod]
        public async Task LexicalaClient_SearchDefinitions_En_Summer()
        {
            string response = await LoadResponseFromFile("Search_en_summer_definitions.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.SearchDefinitionsAsync("summer", "en");

            result.ResultsPerPage.ShouldBe(10);
            result.Results.Length.ShouldBe(10);
        }

        [TestMethod]
        public async Task LexicalaClient_SearchDefinitions_En_Summer_ResultsHaveEntryId()
        {
            string response = await LoadResponseFromFile("Search_en_summer_definitions.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.SearchDefinitionsAsync("summer", "en");

            result.Results[0].EntryId.ShouldBe("EN00009942");
            result.Results[0].Definition.ShouldBe("a vacation from school or college in the spring");
            result.Results[0].Pos.ShouldBe("noun");
            result.Results[0].SenseId.ShouldBe("EN_SE4458d852734b");
        }

        [TestMethod]
        public async Task LexicalaClient_SearchDefinitions_En_Summer_HeadwordStringDeserializes()
        {
            string response = await LoadResponseFromFile("Search_en_summer_definitions.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.SearchDefinitionsAsync("summer", "en");

            result.Results[0].Headword.Headword.Text.ShouldBe("spring break");
        }

        [TestMethod]
        public async Task LexicalaClient_SearchDefinitions_En_Summer_ResultWithoutHeadword()
        {
            string response = await LoadResponseFromFile("Search_en_summer_definitions.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.SearchDefinitionsAsync("summer", "en");

            // Index 7 has no headword in the response
            result.Results[7].EntryId.ShouldBe("EN00010306");
            result.Results[7].Headword.Headword.ShouldBeNull();
            result.Results[7].Definition.ShouldBe("the period during the summer when schools, universities, etc. are closed");
        }

        [TestMethod]
        public async Task LexicalaClient_SearchDefinitions_BuildsCorrectQuery()
        {
            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage("{\"results_per_page\":10,\"results\":[]}"));

            await Client.SearchDefinitionsAsync("summer", "en");

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("search-by-definitions") &&
                    req.RequestUri.ToString().Contains("text=summer") &&
                    req.RequestUri.ToString().Contains("language=en")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
