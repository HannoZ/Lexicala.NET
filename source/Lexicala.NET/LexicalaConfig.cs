using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Lexicala.NET.Configuration
{
    public class LexicalaConfig
    {
        public static readonly Uri BaseAddress = new Uri("https://dictapi.lexicala.com");

        public LexicalaConfig()
        {
            
        }

        public LexicalaConfig(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }

        public AuthenticationHeaderValue CreateAuthenticationHeader()
        {
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{Username}:{Password}")));
        }

        public HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = BaseAddress
            };
            client.DefaultRequestHeaders.Authorization = CreateAuthenticationHeader();

            return client;
        }

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