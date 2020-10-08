using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lexicala.NET.Response.Entries.JsonConverters
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
                PronunciationObjectConverter.Singleton,
                PosConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            }
        };
    }
}