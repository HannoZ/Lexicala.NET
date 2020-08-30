using Lexicala.NET.Client;
using Lexicala.NET.Client.Response;
using Lexicala.NET.Client.Response.Entries;
using Lexicala.NET.Client.Response.Entries.JsonConverters;
using Lexicala.NET.Client.Response.Languages;
using Lexicala.NET.Client.Response.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using Shouldly;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lexicala.NET.Parser.Tests
{
    [TestClass]
    public class LexicalaSearchParserTests
    {
        private LexicalaSearchParser _lexicalaSearchParser;
        private AutoMocker _mocker;

        [TestInitialize]
        public void Initialize()
        {
            _mocker = new AutoMocker(MockBehavior.Loose);
            _lexicalaSearchParser = _mocker.CreateInstance<LexicalaSearchParser>();
        }

        [TestMethod]
        public async Task LexicalaDetailsLoader_SearchAsync_1()
        {
            string searchResult = await LoadResponseFromFile("Search_es_blando_analyzed.json");
            string entry1 = await LoadResponseFromFile("ES_DE00008087.json");
            string entry2 = await LoadResponseFromFile("ES_DE00008088.json");
            
            var apiResult = JsonConvert.DeserializeObject<SearchResponse>(searchResult, SearchResponseJsonConverter.Settings);
            apiResult.Metadata = new ResponseMetadata();
            var entryResult1 = JsonConvert.DeserializeObject<Entry>(entry1, EntryResponseJsonConverter.Settings);
            entryResult1.Metadata = new ResponseMetadata();
            var entryResult2 = JsonConvert.DeserializeObject<Entry>(entry2, EntryResponseJsonConverter.Settings);
            entryResult2.Metadata = new ResponseMetadata();

            var languagesResponse = new LanguagesResponse()
            {
                Resources = new Resources()
                {
                    Global = new Resource()
                    {
                        SourceLanguages = new[]
                        {
                            "en", "es"
                        }
                    }
                }
            };
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.LanguagesAsync())
                .ReturnsAsync(languagesResponse);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.BasicSearchAsync("test", "es", null))
                .ReturnsAsync(apiResult);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008088", null))
                .ReturnsAsync(entryResult2);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008087", null))
                .ReturnsAsync(entryResult1);

            _mocker.GetMock<IMemoryCache>()
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(_mocker.GetMock<ICacheEntry>().Object);

            // ACT
            var result = await _lexicalaSearchParser.SearchAsync("test", "es");

            // ASSERT
            result.Summary("nl").ShouldBe("blandir: zwaaien | blando/blanda: zacht, toegeeflijk, laf");
            result.Results.SelectMany(r => r.Stems).ShouldNotBeEmpty();
        }


        private static Task<string> LoadResponseFromFile(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var resourceStream = asm.GetManifestResourceStream($"Lexicala.NET.Parser.Tests.Resources.{fileName}");

            if (resourceStream != null)
            {
                using var reader = new StreamReader(resourceStream);
                return reader.ReadToEndAsync();
            }

            return Task.FromResult(string.Empty);
        }
    }
}
