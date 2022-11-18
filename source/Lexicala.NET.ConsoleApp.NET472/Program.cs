using Lexicala.NET.Configuration;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lexicala.NET.Autofac;
using Lexicala.NET.Response.Languages;
using Microsoft.Extensions.Caching.Memory;
using Lexicala.NET.Parsing;

namespace Lexicala.NET.ConsoleApp.NET472
{
    /// <summary>
    /// This is an example of how the LexicalaClient can be used, in combination with Autofac as IoC provider. 
    /// </summary>
    class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new LexicalaConfig(ConfigurationManager.AppSettings["username"],  ConfigurationManager.AppSettings["password"]);
            var container = DependencyRegistration.RegisterLexixala(config);
            var lexicalaClient = container.Resolve<ILexicalaClient>();
            var memCache = container.Resolve<IMemoryCache>();
            var parser = container.Resolve<ILexicalaSearchParser>();

            string input = string.Empty;
            while (input != ConsoleKey.Q.ToString())
            {
                Console.WriteLine("Enter search query: searchterm sourcelang targetlang (eg: estar es en). Q to exit");
                input = Console.ReadLine();
                if (input == null)
                {
                    continue;
                }
                
                var tokens = input.Split(' ');
                
                if (tokens.Length != 3)
                {
                    continue;
                }

                var searchTerm = tokens[0];
                var srcLang = tokens[1];
                var tgtLang = tokens[2];
                if (srcLang.Length != 2 || tgtLang.Length != 2)
                {
                    Console.WriteLine("Language must be 2 characters");
                    continue;
                }

                if (!memCache.TryGetValue("languages", out Resource languages))
                {
                    var response = await lexicalaClient.LanguagesAsync();
                    languages = response.Resources.Global;
                    memCache.Set("languages", languages);
                }

                if (!languages.SourceLanguages.Contains(srcLang))
                {
                    Console.WriteLine("Source language not available");
                    continue;
                }

                if (!languages.TargetLanguages.Contains(tgtLang))
                {
                    Console.WriteLine("Target language not available");
                    continue;
                }

                try
                {
                    var searchResponse = await parser.SearchAsync(searchTerm, srcLang, new[]{tgtLang}); /*await lexicalaClient.BasicSearchAsync(searchTerm, srcLang);*/
                    bool hasTranslation = false;
                    foreach (var result in searchResponse.Results)
                    {
                        var entry = await lexicalaClient.GetEntryAsync(result.Id);
                        foreach (var sense in entry.Senses)
                        {
                            Console.WriteLine(sense.Definition);
                            if (sense.Translations.ContainsKey(tgtLang))
                            {
                                hasTranslation = true;
                                var senseTranslation = sense.Translations[tgtLang];
                                if (senseTranslation.Language != null)
                                {
                                    Console.WriteLine(senseTranslation.Language?.Text);
                                }
                                else
                                {
                                    Console.WriteLine(string.Join(", ", senseTranslation.Languages.Select(l => l.Text)));
                                }
                            }
                        }
                    }

                    if (!searchResponse.Results.Any())
                    {
                        Console.WriteLine("No results");
                    }
                    else if (!hasTranslation)
                    {
                        Console.WriteLine("No translations available for target language " + tgtLang);
                    }

                    Console.WriteLine("Api calls remaining: " + searchResponse.Metadata.RateLimits.DailyLimitRemaining);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


    }
}
