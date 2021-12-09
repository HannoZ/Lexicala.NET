using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class CommonLanguageObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(LanguageObject) || t == typeof(LanguageObject?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<Language>(reader);
                    return new LanguageObject { Language = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<Language[]>(reader);
                    return new LanguageObject { Languages = arrayValue };
            }
            throw new Exception("Cannot unmarshal type LanguageObject");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (LanguageObject)untypedValue;
            if (value.Languages != null)
            {
                serializer.Serialize(writer, value.Languages);
                return;
            }
            if (value.Language != null)
            {
                serializer.Serialize(writer, value.Language);
                return;
            }
            throw new Exception("Cannot marshal type LanguageObject");
        }

        public static readonly CommonLanguageObjectConverter Singleton = new CommonLanguageObjectConverter();
    }
}