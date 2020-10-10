using System;
using System.Net.Http.Headers;
using Lexicala.NET.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lexicala.NET.Parsing
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
            services.AddTransient<ILexicalaSearchParser, LexicalaSearchParser>();

            return services;
        }
    }
}