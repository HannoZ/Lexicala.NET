using Lexicala.NET.Configuration;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Lexicala.NET.ConsoleApp.NET472
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new LexicalaConfig(ConfigurationManager.AppSettings["username"],  ConfigurationManager.AppSettings["password"]);
            var httpClient = config.CreateHttpClient();
            var lexicalaClient = new LexicalaClient(httpClient);


            string input = string.Empty;
            while (input != ConsoleKey.Q.ToString())
            {
                Console.WriteLine("Enter search query: searchterm sourcelang (eg: estar es en). Q to exit");
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

                try
                {
                    var searchResponse = await lexicalaClient.BasicSearchAsync(searchTerm, srcLang);
                    foreach (var result in searchResponse.Results)
                    {
                        var entry = await lexicalaClient.GetEntryAsync(result.Id);
                        foreach (var sense in entry.Senses)
                        {
                            if (sense.Translations.ContainsKey(tgtLang))
                            {
                                Console.WriteLine(sense.Translations[tgtLang].Language?.Text);
                            }
                        }
                    }

                    if (!searchResponse.Results.Any())
                    {
                        Console.WriteLine("No results");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


    }
}
