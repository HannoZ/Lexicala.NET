using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using Moq.Protected;
using Shouldly;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Client.Request;
using Lexicala.NET.Client.Response;

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
        [ExpectedException(typeof(ArgumentException))] // ASSERT
        public async Task LexicalaClient_BasicSearch_InvalidLanguageCode_ThrowsException()
        {
            // ACT
            await _client.BasicSearchAsync("searchText", "ess");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))] // ASSERT
        public async Task LexicalaClient_BasicSearch_EmptySearchText_ThrowsException()
        {
            // ACT
            await _client.BasicSearchAsync("", "es");
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

            // ASSERT
            result.Results.Length.ShouldBe(3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))] // ASSERT
        public async Task LexicalaClient_AdvancedSearch_InvalidLanguageCode_ThrowsException()
        {
            // ACT
            await _client.AdvancedSearchAsync(new AdvancedSearchRequest());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))] // ASSERT
        public async Task LexicalaClient_AdvancedSearch_EmptySearchText_ThrowsException()
        {
            // ACT
            await _client.AdvancedSearchAsync(new AdvancedSearchRequest{Language = "xx"});
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_AllDefaults()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text"
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global"
                   ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Source()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Source = Sources.Password
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=password"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Analyzed()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Analyzed = true
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&analyzed=true"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Monosemous()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Monosemous = true
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&monosemous=true"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Polysemous()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Polysemous = true
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&polysemous=true"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Morph()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Morph = true
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&morph=true"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Pos()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Pos = "noun"
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&pos=noun"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Number()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Number = "plural"
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&number=plural"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Gender()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Gender = "masculine"
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&gender=masculine"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Subcategorization()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Subcategorization = "feminine"
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&subcategorization=feminine"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Page_OtherThanDefault()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Page = 2
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&page=2"
                ), ItExpr.IsAny<CancellationToken>());
        }
        
        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Page_Default_NotInQuerystring()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Page = 1
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Page_Invalid_NotInQuerystring()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Page = 0
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_OtherThanDefault()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                PageLength = 1
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&page-length=1"
                ), ItExpr.IsAny<CancellationToken>());
        }


        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_Default_NotInQuerystring()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                PageLength = 10
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_Invalid_Low_NotInQuerystring()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                PageLength = 0
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_Invalid_High_NotInQuerystring()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                PageLength = 31
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Sample()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Sample = 1
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global&sample=1"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Sample_NotGreaterThanZero_NotInQueryString()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var searchRequest = new AdvancedSearchRequest
            {
                Language = "xx",
                SearchText = "text",
                Sample = 0
            };

            // ACT 
            await _client.AdvancedSearchAsync(searchRequest);

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString() == "http://www.tempuri.org/search?language=xx&text=text&source=global"
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_Search_ETag()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));
            
            // ACT 
            await _client.BasicSearchAsync("text", "xx", "W/\"abc-OfxtVSoa\"");

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("If-None-Match")
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_Search_Metadata()
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            var responseMessage = SetupOkResponseMessage(response);
            responseMessage.Headers.ETag = new EntityTagHeaderValue("\"abc-OfxtVSoa\"");
            responseMessage.Headers.Add(ResponseHeaders.HeaderDailyLimitRemaining, "100");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitDailyLimit, "1000");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitLimit, "10");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRemaining, "5");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // ACT
            var result = await _client.BasicSearchAsync("text", "xx");

            // ASSERT
            result.Metadata.ETag.ShouldBe("\"abc-OfxtVSoa\"");
            result.Metadata.RateLimits.DailyLimitRemaining.ShouldBe(100);
            result.Metadata.RateLimits.DailyLimit.ShouldBe(1000);
            result.Metadata.RateLimits.Limit.ShouldBe(10);
            result.Metadata.RateLimits.Remaining.ShouldBe(5);
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

            // ASSERT
            result.Id.ShouldBe("EN_DE00009032");
            result.Senses.Length.ShouldBe(12);
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_EN_DE00009032()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.GetEntryAsync("EN_DE00009032");

            // ASSERT
            result.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_ES_DE00008087()
        {
            string response = await LoadResponseFromFile("Entry_ES_DE00008087.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.GetEntryAsync("ES_DE00008087");

            // ASSERT
            result.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_ES_DE00008089()
        {
            string response = await LoadResponseFromFile("Entry_ES_DE00008088.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.GetEntryAsync("ES_DE00008088");

            // ASSERT
            result.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_NL_DE00006941()
        {
            string response = await LoadResponseFromFile("Entry_NL_DE00006941.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.GetEntryAsync("NL_DE00006941");

            // ASSERT
            result.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_PW00012363()
        {
            string response = await LoadResponseFromFile("Entry_PW00012363.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.GetEntryAsync("PW00012363");

            // ASSERT
            result.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task LexicalaClient_CanDeserializeEntry_RDE00032314_1()
        {
            string response = await LoadResponseFromFile("Entry_RDE00032314_1.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            var result = await _client.GetEntryAsync("RDE00032314_1");

            // ASSERT
            result.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task LexicalaClient_GetEntry_ETag()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            // ACT
            await _client.GetEntryAsync("EN_DE00009032", "W/\"abc-OfxtVSoa\"");

            // ASSERT
            _handlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("If-None-Match")
                ), ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_GetEntry_Metadata()
        {
            string response = await LoadResponseFromFile("Entry_EN_DE00009032.json");

            var responseMessage = SetupOkResponseMessage(response);
            responseMessage.Headers.ETag = new EntityTagHeaderValue("\"abc-OfxtVSoa\"");
            responseMessage.Headers.Add(ResponseHeaders.HeaderDailyLimitRemaining, "100");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitDailyLimit, "1000");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitLimit, "10");
            responseMessage.Headers.Add(ResponseHeaders.HeaderRateLimitRemaining, "5");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // ACT
            var result = await _client.GetEntryAsync("EN_DE00009032");

            // ASSERT
            result.Metadata.ETag.ShouldBe("\"abc-OfxtVSoa\"");
            result.Metadata.RateLimits.DailyLimitRemaining.ShouldBe(100);
            result.Metadata.RateLimits.DailyLimit.ShouldBe(1000);
            result.Metadata.RateLimits.Limit.ShouldBe(10);
            result.Metadata.RateLimits.Remaining.ShouldBe(5);
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
