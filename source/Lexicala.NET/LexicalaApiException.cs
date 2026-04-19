using System;
using System.Net;
using Lexicala.NET.Response;

namespace Lexicala.NET
{
    /// <summary>
    /// Represents an error response returned by the Lexicala API.
    /// </summary>
    public class LexicalaApiException : Exception
    {
        public LexicalaApiException(string message, HttpStatusCode statusCode, string content, ResponseMetadata metadata)
            : base(message)
        {
            StatusCode = statusCode;
            Content = content;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the HTTP status code returned by the API.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the raw response content returned by the API.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets parsed response metadata, including rate limit headers.
        /// </summary>
        public ResponseMetadata Metadata { get; }
    }
}
