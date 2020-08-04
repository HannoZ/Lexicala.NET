using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Entries.JsonConverters
{
    internal class JapaneseObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(JapaneseObject) || t == typeof(JapaneseObject?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<Japanese>(reader);
                    return new JapaneseObject { Ja = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<Japanese[]>(reader);
                    return new JapaneseObject { JaArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type JapaneseObject");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (JapaneseObject)untypedValue;
            if (value.JaArray != null)
            {
                serializer.Serialize(writer, value.JaArray);
                return;
            }
            if (value.Ja != null)
            {
                serializer.Serialize(writer, value.Ja);
                return;
            }
            throw new Exception("Cannot marshal type JapaneseObject");
        }

        public static readonly JapaneseObjectConverter Singleton = new JapaneseObjectConverter();
    }
}