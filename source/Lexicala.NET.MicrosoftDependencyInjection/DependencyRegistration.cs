using Lexicala.NET.Configuration;
using Lexicala.NET.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lexicala.NET.MicrosoftDependencyInjection
{
    public static class DependencyRegistration
    {
        public static IServiceCollection RegisterLexicala(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("Lexicala").Get<LexicalaConfig>();
            return RegisterLexicala(services, config);
        }

        public static IServiceCollection RegisterLexicala(this IServiceCollection services, LexicalaConfig config)
        {
            services.AddHttpClient<ILexicalaClient, LexicalaClient>(client =>
            {
                client.BaseAddress = LexicalaConfig.BaseAddress;
                client.DefaultRequestHeaders.Authorization = config.CreateAuthenticationHeader();
            });

            services.AddMemoryCache();
            services.AddSingleton<ILexicalaSearchParser, LexicalaSearchParser>();

            return services;
        }
    }
}