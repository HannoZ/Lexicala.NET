using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class PronunciationObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(PronunciationObject) || t == typeof(PronunciationObject?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<Pronunciation>(reader);
                    return new PronunciationObject { Pronunciation = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<Pronunciation[]>(reader);
                    return new PronunciationObject { PronunciationArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type PronunciationObject");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (PronunciationObject)untypedValue;
            if (value.PronunciationArray != null)
            {
                serializer.Serialize(writer, value.PronunciationArray);
                return;
            }
            if (value.Pronunciation != null)
            {
                serializer.Serialize(writer, value.Pronunciation);
                return;
            }
            throw new Exception("Cannot marshal type PronunciationObject");
        }

        public static readonly PronunciationObjectConverter Singleton = new PronunciationObjectConverter();
    }
}