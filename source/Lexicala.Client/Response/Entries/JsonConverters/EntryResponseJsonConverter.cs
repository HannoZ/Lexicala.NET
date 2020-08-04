using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lexicala.NET.Client.Response.Entries.JsonConverters
{
    public static class EntryResponseJsonConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = new List<JsonConverter>
            {
                CommonLanguageObjectConverter.Singleton,
                HeadwordObjectConverter.Singleton,
                JapaneseObjectConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            }
        };
    }
}