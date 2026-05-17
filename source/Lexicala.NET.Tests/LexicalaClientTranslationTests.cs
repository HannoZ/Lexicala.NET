using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Shouldly;
using Lexicala.NET.Client.Tests;

namespace Lexicala.NET.Tests
{
    [TestClass]
    public class LexicalaClientTranslationTests : LexicalaClientTestBase
    {
        [TestMethod]
        public async Task LexicalaClient_TranslatePhrase_EmptyText_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.TranslatePhraseAsync("", "de"));
        }

        [TestMethod]
        public async Task LexicalaClient_TranslatePhrase_InvalidTargetLanguage_ThrowsException()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await Client.TranslatePhraseAsync("house", "deu"));
        }

        [TestMethod]
        public async Task LexicalaClient_TranslateTo_UsesExpectedEndpoint()
        {
            const string response = "{\"n_results\":0,\"page_number\":1,\"results_per_page\":10,\"n_pages\":0,\"results\":[]}";

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.TranslateToAsync("house", "de", "en");

            result.NResults.ShouldBe(0);
            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/translate-to?target_language=de&text=house&language=en"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_TranslateExample_UsesExpectedEndpoint()
        {
            const string response = "{\"n_results\":0,\"page_number\":1,\"results_per_page\":10,\"n_pages\":0,\"results\":[]}";

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.TranslateExampleAsync("A house is big.", "de", "en");

            result.PageNumber.ShouldBe(1);
            HandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://www.tempuri.org/translate-example?target_language=de&text=A house is big.&language=en"),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LexicalaClient_TranslatePhrase_ParsesResults()
        {
            const string response = "{\"n_results\":1,\"page_number\":1,\"results_per_page\":10,\"n_pages\":1,\"results\":[{\"text\":\"Haus\"}]}";

            HandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(response));

            var result = await Client.TranslatePhraseAsync("house", "de");

            result.NResults.ShouldBe(1);
            result.Results.Length.ShouldBe(1);
            result.Results[0].GetProperty("text").GetString().ShouldBe("Haus");
        }

        [TestMethod]
        public async Task LexicalaClient_GetEntryWithTranslations_EnrichesSenseAndExample()
        {
            InitializeClient(useLiteEndpoints: true);

            const string entryResponse = "{\"id\":\"ES_TEST\",\"source\":\"global\",\"language\":\"es\",\"version\":1,\"headword\":{\"text\":\"casa\"},\"senses\":[{\"id\":\"ES_SE_TEST\",\"definition\":\"vivienda\",\"translations\":{},\"examples\":[{\"text\":\"La casa es grande.\"}]}]}";
            const string translateToResponse = "{\"n_results\":1,\"page_number\":1,\"results_per_page\":10,\"n_pages\":1,\"results\":[{\"text\":\"house\"}]}";
            const string translateExampleResponse = "{\"n_results\":1,\"page_number\":1,\"results_per_page\":10,\"n_pages\":1,\"results\":[{\"text\":\"The house is big.\"}]}";

            HandlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(entryResponse))
                .ReturnsAsync(SetupOkResponseMessage(translateToResponse))
                .ReturnsAsync(SetupOkResponseMessage(translateExampleResponse));

            var result = await Client.GetEntryWithTranslationsAsync("ES_TEST", "en");

            result.Senses[0].Translations.ContainsKey("en").ShouldBeTrue();
            result.Senses[0].Translations["en"].Translation.Text.ShouldBe("house");
            result.Senses[0].Examples[0].Translations.ContainsKey("en").ShouldBeTrue();
            result.Senses[0].Examples[0].Translations["en"].Translation.Text.ShouldBe("The house is big.");
        }

        [TestMethod]
        public async Task LexicalaClient_GetSenseWithTranslations_EnrichesSenseAndExample()
        {
            const string senseResponse = "{\"id\":\"ES_SE_TEST\",\"definition\":\"casa\",\"translations\":{},\"examples\":[{\"text\":\"La casa es grande.\"}]}";
            const string translateToResponse = "{\"n_results\":1,\"page_number\":1,\"results_per_page\":10,\"n_pages\":1,\"results\":[{\"text\":\"house\"}]}";
            const string translateExampleResponse = "{\"n_results\":1,\"page_number\":1,\"results_per_page\":10,\"n_pages\":1,\"results\":[{\"text\":\"The house is big.\"}]}";

            HandlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(SetupOkResponseMessage(senseResponse))
                .ReturnsAsync(SetupOkResponseMessage(translateToResponse))
                .ReturnsAsync(SetupOkResponseMessage(translateExampleResponse));

            var result = await Client.GetSenseWithTranslationsAsync("ES_SE_TEST", "en");

            result.Translations.ContainsKey("en").ShouldBeTrue();
            result.Translations["en"].Translation.Text.ShouldBe("house");
            result.Examples[0].Translations.ContainsKey("en").ShouldBeTrue();
            result.Examples[0].Translations["en"].Translation.Text.ShouldBe("The house is big.");
        }
    }
}
