using Lexicala.NET.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Lexicala.NET.MicrosoftDependencyInjection;

namespace Lexicala.NET.ConsoleApp
{
    public class Program
    {
        private static IServiceProvider _serviceProvider;

        public static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();


            try
            {
                RegisterServices(configuration);

                await ExecuteMainLoop();
            }
            finally
            {
                DisposeServices();
            }
        }

        private static async Task ExecuteMainLoop()
        {
            var parser = _serviceProvider.GetService<ILexicalaSearchParser>();

            string input = string.Empty;
            while (input != ConsoleKey.Q.ToString())
            {
                Console.WriteLine("Enter an entry ID directly, or enter search query: searchterm sourcelang targetlang (eg: estar es en). Q to exit");
                input = Console.ReadLine();
                if (input == null)
                {
                    continue;
                }

                
                var tokens = input.Split(' ');
                if (tokens.Length == 1)
                {
                    var entry = await parser.GetEntryAsync(tokens[0], "ar", "en", "es", "nl", "zh" );
                }
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
                    var result = await parser.SearchAsync(searchTerm, srcLang);
                    var summary = result.Summary(tgtLang);
                    if (string.IsNullOrEmpty(summary))
                    {
                        summary = "No results";
                    }
                    Console.WriteLine(summary);

                    //result = await parser.SearchAsync(searchTerm, srcLang, result.ETag);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void RegisterServices(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();
            services.RegisterLexicala(configuration);

            _serviceProvider = services.BuildServiceProvider(true);
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
