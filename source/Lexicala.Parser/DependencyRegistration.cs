using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;
using Lexicala.NET.Client;

namespace Lexicala.NET.Parser
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
                client.BaseAddress = new Uri("https://dictapi.lexicala.com");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{config.Username}:{config.Password}")));
            });

            services.AddMemoryCache();

            services.AddTransient<ILexicalaSearchParser, LexicalaSearchParser>();

            return services;
        }
    }
}