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

        public const string RapidApiKeyHeader = "X-RapidAPI-Key";
        public const string RapidApiHostHeader = "X-RapidAPI-Host";
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
        /// The RapidAPI Api key.
        /// </summary>
        public string ApiKey { get; set; }

    }
}