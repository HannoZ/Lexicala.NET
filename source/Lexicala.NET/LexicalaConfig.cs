using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Lexicala.NET.Configuration
{
    /// <summary>
    /// This class is used to setup and configure communication with the Lexicala API. 
    /// </summary>
    public class LexicalaConfig
    {
        /// <summary>
        /// The Lexicala API base address.
        /// </summary>
        public static readonly Uri BaseAddress = new Uri("https://dictapi.lexicala.com");

        /// <summary>
        /// Creates a new instance of the <see cref="LexicalaConfig"/> class.
        /// </summary>
        public LexicalaConfig()
        {
            
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LexicalaConfig"/> class, with specified username and password for the API.
        /// </summary>
        public LexicalaConfig(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// A username for the Lexicala API.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The password that belongs to the specified <see cref="Username"/>.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// This method creates the authentication header that must be added to each request to the Lexicala API.
        /// </summary>
        public AuthenticationHeaderValue CreateAuthenticationHeader()
        {
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{Username}:{Password}")));
        }

        /// <summary>
        /// This method creates a <see cref="HttpClient"/> instance that is already setup and ready for interaction with the Lexicala API.
        /// </summary>
        public HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = BaseAddress
            };
            client.DefaultRequestHeaders.Authorization = CreateAuthenticationHeader();

            return client;
        }

        /// <summary>
        /// This method creates a <see cref="HttpClient"/> instance with a specific <see cref="HttpClientHandler"/> that is already setup and ready for interaction with the Lexicala API.
        /// </summary>
        public HttpClient CreateHttpClient(HttpClientHandler handler)
        {
            var client = new HttpClient(handler)
            {
                BaseAddress = BaseAddress
            };
            client.DefaultRequestHeaders.Authorization = CreateAuthenticationHeader();

            return client;
        }
    }
}