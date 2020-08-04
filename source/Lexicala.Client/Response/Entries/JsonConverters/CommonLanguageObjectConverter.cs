using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries.JsonConverters
{
    internal class CommonLanguageObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(CommonLanguageObject) || t == typeof(CommonLanguageObject?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<CommonLanguage>(reader);
                    return new CommonLanguageObject { CommonLanguage = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<CommonLanguage[]>(reader);
                    return new CommonLanguageObject { CommonLanguageObjectArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type CommonLanguageObject");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (CommonLanguageObject)untypedValue;
            if (value.CommonLanguageObjectArray != null)
            {
                serializer.Serialize(writer, value.CommonLanguageObjectArray);
                return;
            }
            if (value.CommonLanguage != null)
            {
                serializer.Serialize(writer, value.CommonLanguage);
                return;
            }
            throw new Exception("Cannot marshal type CommonLanguageObject");
        }

        public static readonly CommonLanguageObjectConverter Singleton = new CommonLanguageObjectConverter();
    }
}