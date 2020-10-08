using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lexicala.NET.Response.Search
{
    public static class SearchResponseJsonConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                HeadwordObjectConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}