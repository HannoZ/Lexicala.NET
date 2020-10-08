using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Search
{
    internal class HeadwordObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(HeadwordObject) || t == typeof(HeadwordObject?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<Headword>(reader);
                    return new HeadwordObject { Headword = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<Headword[]>(reader);
                    return new HeadwordObject { HeadwordElementArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type HeadwordObject");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (HeadwordObject)untypedValue;
            if (value.HeadwordElementArray != null)
            {
                serializer.Serialize(writer, value.HeadwordElementArray);
                return;
            }
            if (value.Headword != null)
            {
                serializer.Serialize(writer, value.Headword);
                return;
            }
            throw new Exception("Cannot marshal type HeadwordObject");
        }

        public static readonly HeadwordObjectConverter Singleton = new HeadwordObjectConverter();
    }
}