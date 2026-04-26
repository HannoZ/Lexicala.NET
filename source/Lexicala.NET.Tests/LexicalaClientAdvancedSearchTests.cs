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
    public class LexicalaClientAdvancedSearchTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_InvalidLanguageCode_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.AdvancedSearchAsync(new AdvancedSearchRequest()));
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_EmptySearchText_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.AdvancedSearchAsync(new AdvancedSearchRequest { Language = "xx" }));
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_AllDefaults()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text" },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Source()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Source = Sources.Password },
                "http://www.tempuri.org/search?language=xx&text=text&source=password");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Analyzed()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Analyzed = true },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&analyzed=true");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Monosemous()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Monosemous = true },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&monosemous=true");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Polysemous()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Polysemous = true },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&polysemous=true");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Morph()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Morph = true },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&morph=true");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Pos()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Pos = "noun" },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&pos=noun");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Number()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Number = "plural" },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&number=plural");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Gender()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Gender = "masculine" },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&gender=masculine");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Subcategorization()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Subcategorization = "feminine" },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&subcategorization=feminine");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Page_OtherThanDefault()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Page = 2 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&page=2");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Page_Default_NotInQuerystring()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Page = 1 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Page_Invalid_NotInQuerystring()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Page = 0 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_OtherThanDefault()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", PageLength = 1 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&page-length=1");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_Default_NotInQuerystring()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", PageLength = 10 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_Invalid_Low_NotInQuerystring()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", PageLength = 0 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_PageLength_Invalid_High_NotInQuerystring()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", PageLength = 31 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Sample()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Sample = 1 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&sample=1");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Sample_NotGreaterThanZero_NotInQueryString()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Sample = 0 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Synonyms_InQueryString()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Synonyms = true },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&synonyms=true");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Antonyms_InQueryString()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Antonyms = true },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&antonyms=true");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_MultipleParameters_InQueryString()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest
                {
                    Language = "xx",
                    SearchText = "text",
                    Analyzed = true,
                    Monosemous = true,
                    Synonyms = true,
                    Antonyms = true,
                    Pos = "noun",
                    Page = 2,
                    PageLength = 5
                },
                "http://www.tempuri.org/search?language=xx&text=text&source=global&analyzed=true&monosemous=true&synonyms=true&antonyms=true&pos=noun&page=2&page-length=5");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Page_Excessive_NotInQueryString()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Page = 1001 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        [TestMethod]
        public async Task LexicalaClient_AdvancedSearch_Sample_Excessive_NotInQueryString()
        {
            await AssertAdvancedSearchQuery(new AdvancedSearchRequest { Language = "xx", SearchText = "text", Sample = 1001 },
                "http://www.tempuri.org/search?language=xx&text=text&source=global");
        }

        private async Task AssertAdvancedSearchQuery(AdvancedSearchRequest searchRequest, string expectedUri)
        {
            string response = await LoadResponseFromFile("Search_empty.json");

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            await Client.AdvancedSearchAsync(searchRequest);

            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
