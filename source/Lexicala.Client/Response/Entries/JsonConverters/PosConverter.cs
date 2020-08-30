using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries.JsonConverters
{
    internal class PosConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Pos) || t == typeof(Pos?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Pos { PartOfSpeech = stringValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<string[]>(reader);
                    return new Pos { PartOfSpeechArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type PartOfSpeech");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Pos)untypedValue;
            if (value.PartOfSpeech != null)
            {
                serializer.Serialize(writer, value.PartOfSpeech);
                return;
            }
            if (value.PartOfSpeechArray != null)
            {
                serializer.Serialize(writer, value.PartOfSpeechArray);
                return;
            }
            throw new Exception("Cannot marshal type PartOfSpeech");
        }

        public static readonly PosConverter Singleton = new PosConverter();
    }
}