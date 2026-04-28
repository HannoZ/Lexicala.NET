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
        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalaApiException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code returned by the API.</param>
        /// <param name="content">The raw response content returned by the API.</param>
        /// <param name="metadata">Parsed response metadata, including rate limit headers.</param>
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
