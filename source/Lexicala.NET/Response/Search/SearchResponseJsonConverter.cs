using System.Text.Json;

namespace Lexicala.NET.Response.Search
{
    /// <summary>
    /// Provides serializer settings used for search response conversion.
    /// </summary>
    public static class SearchResponseJsonConverter
    {
        /// <summary>
        /// Gets the JSON serializer options used to deserialize search responses.
        /// </summary>
        public static readonly JsonSerializerOptions Settings = JsonSerializerDefaults.Options;
    }
}

