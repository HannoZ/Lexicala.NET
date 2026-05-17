using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lexicala.NET.Parsing;
using Lexicala.NET.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Lexicala.NET
{
    /// <summary>
    /// Provides dependency injection registration helpers for Lexicala services.
    /// </summary>
    public static class DependencyRegistration
    {
        /// <param name="services">The service collection.</param>
        extension(IServiceCollection services)
        {
            /// <summary>
            /// Registers Lexicala services using configuration from the "Lexicala" section.
            /// </summary>
            /// <param name="configuration">The application configuration.</param>
            /// <returns>The updated service collection.</returns>
            public IServiceCollection RegisterLexicala(IConfiguration configuration)
            {
                services.Configure<LexicalaConfig>(configuration.GetSection("Lexicala"));
                return RegisterLexicala(services);
            }

            /// <summary>
            /// Registers Lexicala services using an explicit configuration object.
            /// </summary>
            /// <param name="config">The Lexicala configuration.</param>
            /// <returns>The updated service collection.</returns>
            public IServiceCollection RegisterLexicala(LexicalaConfig config)
            {
                services.Configure<LexicalaConfig>(o =>
                {
                    o.ApiKey = config.ApiKey;
                    o.UseLiteEndpoints = config.UseLiteEndpoints;
                });
                return RegisterLexicala(services);
            }

            private IServiceCollection RegisterLexicala()
            {
                services.AddHttpClient<ILexicalaClient, LexicalaClient>((provider, client) =>
                    {
                        var config = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LexicalaConfig>>().Value;
                        client.BaseAddress = LexicalaConfig.BaseAddress;
                        client.DefaultRequestHeaders.Add(LexicalaConfig.RapidApiKeyHeader, config.ApiKey);
                        client.DefaultRequestHeaders.Add(LexicalaConfig.RapidApiHostHeader, LexicalaConfig.RapidApiHostValue);
                    })
                    .AddPolicyHandler((serviceProvider, _) =>
                        CreateRetryPolicy(serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<LexicalaClient>()));

                services.AddMemoryCache();
                services.AddSingleton<ILexicalaSearchParser, LexicalaSearchParser>();

                return services;
            }
        }

        /// <summary>
        /// The maximum delay the retry policy will wait between attempts.
        /// If the API signals a reset time greater than this threshold, the request fails
        /// immediately instead of retrying — there is no point waiting longer than this
        /// because any subsequent attempt would also exceed the remaining quota window.
        /// </summary>
        private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(60);

        private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(ILogger logger)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response =>
                {
                    if (response.StatusCode != HttpStatusCode.TooManyRequests)
                    {
                        return false;
                    }

                    // Only retry if the rate-limit reset window fits within our max delay.
                    // If the server says the quota won't reset for longer than MaxRetryDelay,
                    // retrying would never succeed within that window — fail immediately.
                    if (response.Headers.TryGetValues(ResponseHeaders.HeaderRateLimitReset, out var resetValues) &&
                        int.TryParse(resetValues.FirstOrDefault(), out var resetSeconds) &&
                        resetSeconds > (int)MaxRetryDelay.TotalSeconds)
                    {
                        logger.LogWarning(
                            "Rate limit exceeded (HTTP 429). API quota resets in {ResetSeconds}s which exceeds the retry threshold ({ThresholdSec}s). Not retrying.",
                            resetSeconds, (int)MaxRetryDelay.TotalSeconds);
                        return false;
                    }

                    return true;
                })
                .RetryAsync(3, async (outcome, retryAttempt, _) =>
                {
                    var retryDelay = GetRetryDelay(outcome.Result, retryAttempt);

                    if (outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        logger.LogWarning(
                            "Rate limit exceeded (HTTP 429). Waiting {RetryDelaySec}s before retry attempt {RetryAttempt}/3.",
                            (int)retryDelay.TotalSeconds, retryAttempt);
                    }
                    else
                    {
                        logger.LogWarning(
                            outcome.Exception,
                            "Request failed with status {StatusCode}. Waiting {RetryDelaySec}s before retry attempt {RetryAttempt}/3.",
                            outcome.Result?.StatusCode, (int)retryDelay.TotalSeconds, retryAttempt);
                    }

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
                if (response.Headers.RetryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
                {
                    return delta;
                }

                if (response.Headers.RetryAfter?.Date is { } date)
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