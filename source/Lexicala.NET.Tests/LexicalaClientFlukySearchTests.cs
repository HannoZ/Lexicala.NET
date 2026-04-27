using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Shouldly;

namespace Lexicala.NET.Client.Tests
{
    [TestClass]
    public class LexicalaClientFlukySearchTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_FlukySearch_NullSource_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.FlukySearchAsync(source: null));
        }

        [TestMethod]
        public async Task LexicalaClient_FlukySearch_EmptySource_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.FlukySearchAsync(source: string.Empty));
        }

        [TestMethod]
        public async Task LexicalaClient_FlukySearch_InvalidSource_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.FlukySearchAsync(source: "en"));
        }

        [TestMethod]
        public async Task LexicalaClient_FlukySearch_GlobalSource_BuildsCorrectQuery()
        {
            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage("{\"results\":[],\"n_results\":0}"));

            await Client.FlukySearchAsync(source: Sources.Global);

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("source=global")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_FlukySearch_MultiglossSource_BuildsCorrectQuery()
        {
            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage("{\"results\":[],\"n_results\":0}"));

            await Client.FlukySearchAsync(source: "multigloss");

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("source=multigloss")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
