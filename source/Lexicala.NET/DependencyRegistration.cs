using Lexicala.NET.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Lexicala.NET
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
                    client.DefaultRequestHeaders.Add(LexicalaConfig.RapidApiKeyHeader, config.ApiKey);
                    client.DefaultRequestHeaders.Add(LexicalaConfig.RapidApiHostHeader, LexicalaConfig.RapidApiHostValue);
                })
                .AddTransientHttpErrorPolicy(builder => builder.RetryAsync(3));

            services.AddMemoryCache();
            services.AddSingleton<ILexicalaSearchParser, LexicalaSearchParser>();

            return services;
        }
    }
}