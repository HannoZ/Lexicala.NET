using System;

namespace Lexicala.NET
{
    /// <summary>
    /// This class is used to setup and configure communication with the Lexicala API. 
    /// </summary>
    public class LexicalaConfig
    {
        /// <summary>
        /// The Lexicala API base address.
        /// </summary>
        public static readonly Uri BaseAddress = new Uri("https://lexicala1.p.rapidapi.com");

        /// <summary>
        /// HTTP header name used for the RapidAPI key.
        /// </summary>
        public const string RapidApiKeyHeader = "X-RapidAPI-Key";

        /// <summary>
        /// HTTP header name used for the RapidAPI host.
        /// </summary>
        public const string RapidApiHostHeader = "X-RapidAPI-Host";

        /// <summary>
        /// RapidAPI host header value for the Lexicala API.
        /// </summary>
        public const string RapidApiHostValue = "lexicala1.p.rapidapi.com";

        /// <summary>
        /// Creates a new instance of the <see cref="LexicalaConfig"/> class.
        /// </summary>
        public LexicalaConfig()
        {
            
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LexicalaConfig"/> class, with specified API key.
        /// </summary>
        public LexicalaConfig(string apiKey)
        {
            ApiKey = apiKey;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LexicalaConfig"/> class, with specified API key and endpoint mode.
        /// </summary>
        public LexicalaConfig(string apiKey, bool useLiteEndpoints)
        {
            ApiKey = apiKey;
            UseLiteEndpoints = useLiteEndpoints;
        }

        /// <summary>
        /// The RapidAPI Api key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// When true, uses the Lite API variants for entry and sense retrieval/search where available.
        /// </summary>
        public bool UseLiteEndpoints { get; set; }

    }
}