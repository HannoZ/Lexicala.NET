using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class TranslationObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TranslationObject) || t == typeof(TranslationObject?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<Translation>(reader);
                    return new TranslationObject { Translation = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<Translation[]>(reader);
                    return new TranslationObject { Translations = arrayValue };
            }
            throw new Exception("Cannot unmarshal type TranslationObject");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (TranslationObject)untypedValue;
            if (value.Translations != null)
            {
                serializer.Serialize(writer, value.Translations);
                return;
            }
            if (value.Translation != null)
            {
                serializer.Serialize(writer, value.Translation);
                return;
            }
            throw new Exception("Cannot marshal type TranslationObject");
        }

        public static readonly TranslationObjectConverter Singleton = new TranslationObjectConverter();
    }
}