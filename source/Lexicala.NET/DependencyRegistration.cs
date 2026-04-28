using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lexicala.NET.Parsing;
using Lexicala.NET.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Lexicala.NET
{
    /// <summary>
    /// Provides dependency injection registration helpers for Lexicala services.
    /// </summary>
    public static class DependencyRegistration
    {
        /// <summary>
        /// Registers Lexicala services using configuration from the "Lexicala" section.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection RegisterLexicala(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("Lexicala").Get<LexicalaConfig>();
            return RegisterLexicala(services, config);
        }

        /// <summary>
        /// Registers Lexicala services using an explicit configuration object.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="config">The Lexicala configuration.</param>
        /// <returns>The updated service collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="LexicalaConfig.ApiKey"/> is missing.</exception>
        public static IServiceCollection RegisterLexicala(this IServiceCollection services, LexicalaConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                throw new ArgumentException("ApiKey must be provided and cannot be empty", nameof(config.ApiKey));
            }

            services.AddHttpClient<ILexicalaClient, LexicalaClient>(client =>
                {
                    client.BaseAddress = LexicalaConfig.BaseAddress;
                    client.DefaultRequestHeaders.Add(LexicalaConfig.RapidApiKeyHeader, config.ApiKey);
                    client.DefaultRequestHeaders.Add(LexicalaConfig.RapidApiHostHeader, LexicalaConfig.RapidApiHostValue);
                })
                .AddPolicyHandler(CreateRetryPolicy());

            services.AddMemoryCache();
            services.AddSingleton<ILexicalaSearchParser, LexicalaSearchParser>();

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .RetryAsync(3, async (outcome, retryAttempt, _) =>
                {
                    var retryDelay = GetRetryDelay(outcome.Result, retryAttempt);
                    if (retryDelay > TimeSpan.Zero)
                    {
                        await Task.Delay(retryDelay);
                    }
                });
        }

        private static TimeSpan GetRetryDelay(HttpResponseMessage response, int retryAttempt)
        {
            if (response != null)
            {
                if (response.Headers.RetryAfter?.Delta is TimeSpan delta && delta > TimeSpan.Zero)
                {
                    return delta;
                }

                if (response.Headers.RetryAfter?.Date is DateTimeOffset date)
                {
                    var retryAfterDateDelay = date - DateTimeOffset.UtcNow;
                    if (retryAfterDateDelay > TimeSpan.Zero)
                    {
                        return retryAfterDateDelay;
                    }
                }

                if (response.Headers.TryGetValues(ResponseHeaders.HeaderRateLimitReset, out var values))
                {
                    var value = values.FirstOrDefault();
                    if (int.TryParse(value, out var secondsUntilReset) && secondsUntilReset > 0)
                    {
                        return TimeSpan.FromSeconds(secondsUntilReset);
                    }
                }
            }

            // Exponential backoff fallback when no server guidance is available.
            return TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 8));
        }
    }
}