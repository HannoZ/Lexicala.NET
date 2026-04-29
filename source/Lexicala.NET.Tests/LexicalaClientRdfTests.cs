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
    public class LexicalaClientRdfTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_SearchRdf_IncludesSearchRdfEndpoint()
        {
            const string response = "{ \"@context\": {}, \"@graph\": [] }";

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.SearchRdfAsync("text", "xx");

            result.ShouldBe(response);
            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/search-rdf?language=xx&text=text"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_GetRdf_IncludesRdfEndpoint()
        {
            const string response = "{ \"@context\": {}, \"@graph\": [] }";

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.GetRdfAsync("EN_DE00009032");

            result.ShouldBe(response);
            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/rdf/EN_DE00009032"),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
