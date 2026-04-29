using System.Text.Json;

namespace Lexicala.NET.Response.Search
{
    /// <summary>
    /// Provides serializer settings used for search response conversion.
    /// </summary>
    internal static class SearchResponseJsonConverter
    {
        /// <summary>
        /// Gets JSON serializer options used to deserialize search responses.
        /// </summary>
        internal static readonly JsonSerializerOptions Settings = JsonSerializerDefaults.Options;
    }
}

