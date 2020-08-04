using Lexicala.NET.Client;
using Lexicala.NET.Client.Response.Entries;
using Lexicala.NET.Client.Response.Entries.JsonConverters;
using Lexicala.NET.Client.Response.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using Shouldly;
using System.Linq;
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
        public async Task LexicalaDetailsLoader_SearchResult_Basic2()
        {
            string json1 = @"{
  ""n_results"": 2,
  ""page_number"": 1,
  ""results_per_page"": 10,
  ""n_pages"": 1,
  ""available_n_pages"": 1,
  ""results"": [
    {
      ""id"": ""ES_DE00008088"",
      ""language"": ""es"",
      ""headword"": [
        {
          ""text"": ""blando"",
          ""pos"": ""adjective""
        },
        {
          ""text"": ""blanda"",
          ""pos"": ""adjective""
        }
      ],
      ""senses"": [
        {
          ""id"": ""ES_SE00010914"",
          ""definition"": ""que se deforma o corta con facilidad""
        },
        {
          ""id"": ""ES_SE00010915"",
          ""definition"": ""que es flexible""
        },
        {
          ""id"": ""ES_SE00010916"",
          ""definition"": ""que es cobarde""
        }
      ]
    },
    {
      ""id"": ""ES_DE00008087"",
      ""language"": ""es"",
      ""headword"": {
        ""text"": ""blandir"",
        ""pos"": ""verb""
      },
      ""senses"": [
        {
          ""id"": ""ES_SE00010913"",
          ""definition"": ""empuñar un arma amenazantemente""
        }
      ]
    }
  ]
}";

            var entry1 = @"{
  ""id"": ""ES_DE00008088"",
  ""source"": ""global"",
  ""language"": ""es"",
  ""version"": 1,
  ""headword"": [
    {
      ""text"": ""blando"",
      ""pronunciation"": {
        ""value"": ""'blando""
      },
      ""pos"": ""adjective"",
      ""additional_inflections"": [
        ""blanda"",
        ""blandas"",
        ""blandete"",
        ""blandísima"",
        ""blandísimo"",
        ""blandísimos"",
        ""blandita"",
        ""blanditas"",
        ""blandito"",
        ""blanditos"",
        ""blandos"",
        ""blandote""
      ]
    },
    {
      ""text"": ""blanda"",
      ""pronunciation"": {
        ""value"": ""'blanda""
      },
      ""pos"": ""adjective""
    }
  ],
  ""senses"": [
    {
      ""id"": ""ES_SE00010914"",
      ""definition"": ""que se deforma o corta con facilidad"",
      ""range_of_application"": ""cosa material"",
      ""antonyms"": [
        ""duro, rígido""
      ],
      ""translations"": {
        ""br"": {
          ""text"": ""mole""
        },
        ""dk"": [
          {
            ""text"": ""blød""
          },
          {
            ""text"": ""bøjelig""
          }
        ],
        ""en"": {
          ""text"": ""soft""
        },
        ""ja"": {
          ""text"": ""軟（やわ）らかい"",
          ""alternative_scripts"": {
            ""romaji"": ""yawarakai""
          }
        },
        ""nl"": {
          ""text"": ""zacht""
        },
        ""no"": [
          {
            ""text"": ""myk/-t""
          },
          {
            ""text"": ""mør/-t""
          }
        ],
        ""sv"": [
          {
            ""text"": ""mjuk""
          },
          {
            ""text"": ""mör""
          }
        ]
      },
      ""examples"": [
        {
          ""text"": ""La arcilla está blanda aún."",
          ""translations"": {
            ""br"": {
              ""text"": ""A argila ainda está mole.""
            },
            ""dk"": {
              ""text"": ""Leret er stadigvæk blødt.""
            },
            ""en"": {
              ""text"": ""The clay is still soft.""
            },
            ""ja"": {
              ""text"": ""粘土（ねんど）はまだ軟（やわ）らかい。"",
              ""alternative_scripts"": {
                ""romaji"": ""Nendo wa mada yawaarakai.""
              }
            },
            ""nl"": {
              ""text"": ""De klei is nog zacht.""
            },
            ""no"": {
              ""text"": ""Leiren er fremdeles myk.""
            },
            ""sv"": {
              ""text"": ""Leran är fortfarande mjuk.""
            }
          }
        },
        {
          ""text"": ""El barro es una material blando.""
        },
        {
          ""text"": ""Es una carne blanda."",
          ""translations"": {
            ""br"": {
              ""text"": ""É uma carne mole.""
            },
            ""dk"": {
              ""text"": ""Det er mørt kød.""
            },
            ""en"": {
              ""text"": ""It's tender meat.""
            },
            ""ja"": {
              ""text"": ""これは軟（やわ）らかい肉（にく）だ。"",
              ""alternative_scripts"": {
                ""romaji"": ""Kore wa yawarakai niku da.""
              }
            },
            ""nl"": {
              ""text"": ""Het is mals vlees.""
            },
            ""no"": {
              ""text"": ""Det er et mørt kjøtt.""
            },
            ""sv"": {
              ""text"": ""Det är ett mört kött.""
            }
          }
        }
      ]
    },
    {
      ""id"": ""ES_SE00010915"",
      ""definition"": ""que es flexible"",
      ""range_of_application"": ""persona"",
      ""antonyms"": [
        ""duro""
      ],
      ""translations"": {
        ""br"": {
          ""text"": ""mole""
        },
        ""dk"": [
          {
            ""text"": ""blød""
          },
          {
            ""text"": ""rar""
          },
          {
            ""text"": ""eftergivende""
          }
        ],
        ""en"": {
          ""text"": ""soft""
        },
        ""ja"": {
          ""text"": ""甘（あま）い"",
          ""alternative_scripts"": {
            ""romaji"": ""amai""
          }
        },
        ""nl"": {
          ""text"": ""toegeeflijk""
        },
        ""sv"": [
          {
            ""text"": ""mjuk""
          },
          {
            ""text"": ""eftergiven""
          }
        ]
      },
      ""examples"": [
        {
          ""text"": ""La docente es blanda con sus alumnos."",
          ""translations"": {
            ""br"": {
              ""text"": ""A docente é mole com seus alunos.""
            },
            ""dk"": {
              ""text"": ""Underviseren er eftergivende overfor sine studerende.""
            },
            ""en"": {
              ""text"": ""The teacher is soft on her pupils.""
            },
            ""ja"": {
              ""text"": ""その女（おんな）の先生（せんせい）は生徒（せいと）に甘（あま）い。"",
              ""alternative_scripts"": {
                ""romaji"": ""Sono onna no sensee wa seeto ni amai.""
              }
            },
            ""nl"": {
              ""text"": ""De lerares is toegeeflijk met haar leerlingen.""
            },
            ""no"": {
              ""text"": ""Lærerinnen er mild mot elevene sine.""
            },
            ""sv"": {
              ""text"": ""Läraren är mjuk med sina elever.""
            }
          }
        },
        {
          ""text"": ""Su madre es más blanda con él que su padre.""
        }
      ]
    },
    {
      ""id"": ""ES_SE00010916"",
      ""definition"": ""que es cobarde"",
      ""range_of_application"": ""persona"",
      ""translations"": {
        ""br"": {
          ""text"": ""frouxo""
        },
        ""dk"": [
          {
            ""text"": ""kujon""
          },
          {
            ""text"": ""svag""
          }
        ],
        ""en"": [
          {
            ""text"": ""weak""
          },
          {
            ""text"": ""feeble""
          }
        ],
        ""ja"": {
          ""text"": ""気（き）の弱（よわ）い、弱腰（よわごし）の、臆病（おくびょう）な"",
          ""alternative_scripts"": {
            ""romaji"": ""ki no yowai, yowagoshino, okubyoona""
          }
        },
        ""nl"": {
          ""text"": ""laf""
        },
        ""no"": {
          ""text"": ""feig/-t""
        },
        ""sv"": [
          {
            ""text"": ""mesig""
          },
          {
            ""text"": ""feg""
          }
        ]
      },
      ""examples"": [
        {
          ""text"": ""un rival muy blando"",
          ""translations"": {
            ""br"": {
              ""text"": ""um adversário muito frouxo""
            },
            ""dk"": {
              ""text"": ""en svag modstander.""
            },
            ""en"": {
              ""text"": ""a very weak opponent""
            },
            ""ja"": {
              ""text"": ""とても気（き）の弱（よわ）いライバル　"",
              ""alternative_scripts"": {
                ""romaji"": ""totemo ki no yowai raibaru""
              }
            },
            ""nl"": {
              ""text"": ""een zeer laffe rivaal""
            },
            ""no"": {
              ""text"": ""en feig motstander""
            },
            ""sv"": {
              ""text"": ""en mesig rival""
            }
          }
        },
        {
          ""text"": ""unos jugadores muy blandos""
        }
      ]
    }
  ]
}";
            var entry2 = @"{
  ""id"": ""ES_DE00008087"",
  ""source"": ""global"",
  ""language"": ""es"",
  ""version"": 1,
  ""headword"": {
    ""text"": ""blandir"",
    ""pronunciation"": {
      ""value"": ""blan'dir""
    },
    ""pos"": ""verb"",
    ""subcategorization"": ""transitive""
  },
  ""senses"": [
    {
      ""id"": ""ES_SE00010913"",
      ""definition"": ""empuñar un arma amenazantemente"",
      ""translations"": {
        ""br"": {
          ""text"": ""brandir""
        },
        ""dk"": [
          {
            ""text"": ""true med""
          },
          {
            ""text"": ""svinge med""
          }
        ],
        ""en"": {
          ""text"": ""to brandish""
        },
        ""ja"": {
          ""text"": ""武器（ぶき）を振（ふ）りかざす"",
          ""alternative_scripts"": {
            ""romaji"": ""buki o furi kazasu""
          }
        },
        ""nl"": {
          ""text"": ""zwaaien""
        },
        ""no"": {
          ""text"": ""vifte truende""
        },
        ""sv"": {
          ""text"": ""svinga""
        }
      },
      ""examples"": [
        {
          ""text"": ""Blandió la navaja."",
          ""translations"": {
            ""br"": {
              ""text"": ""Brandiu a navalha.""
            },
            ""dk"": {
              ""text"": ""Han truede med kniven.""
            },
            ""en"": {
              ""text"": ""He brandished the knife.""
            },
            ""ja"": {
              ""text"": ""彼（かれ）は小刀（こがたな）を振（ふ）りかざした。"",
              ""alternative_scripts"": {
                ""romaji"": ""Kare wa kogatana o furi kazashita.""
              }
            },
            ""nl"": {
              ""text"": ""Hij zwaaide met het zakmes.""
            },
            ""no"": {
              ""text"": ""Han viftet med lommekniven.""
            },
            ""sv"": {
              ""text"": ""Han svingde kniven.""
            }
          }
        },
        {
          ""text"": ""blandir la espada""
        }
      ]
    }
  ]
}";

            var apiResult = JsonConvert.DeserializeObject<SearchResponse>(json1, SearchResponseJsonConverter.Settings);
            var entryResult1 = JsonConvert.DeserializeObject<Entry>(entry1, EntryResponseJsonConverter.Settings);
            var entryResult2 = JsonConvert.DeserializeObject<Entry>(entry2, EntryResponseJsonConverter.Settings);

            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.BasicSearchAsync("test", "es", null))
                .ReturnsAsync(apiResult);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008088", null))
                .ReturnsAsync(entryResult1);
            _mocker.GetMock<ILexicalaClient>()
                .Setup(m => m.GetEntryAsync("ES_DE00008087", null))
                .ReturnsAsync(entryResult2);

            // ACT
            var result = await _lexicalaSearchParser.SearchAsync("test", "es");

            // ASSERT
            result.Summary("nl").ShouldBe("blandir: zwaaien | blando/blanda: zacht, toegeeflijk, laf");
            result.Results.SelectMany(r => r.Stems).ShouldNotBeEmpty();
        }
    }
}
