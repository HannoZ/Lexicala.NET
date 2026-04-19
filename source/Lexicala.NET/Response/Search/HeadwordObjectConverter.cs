using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Search
{
    internal class HeadwordObjectConverter : JsonConverter<HeadwordObject>
    {
        public override HeadwordObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<Headword>(ref reader, options);
                    return new HeadwordObject { Headword = objectValue };
                case JsonTokenType.StartArray:
                    var arrayValue = JsonSerializer.Deserialize<Headword[]>(ref reader, options);
                    return new HeadwordObject { HeadwordElementArray = arrayValue };
            }

            throw new JsonException("Cannot unmarshal type HeadwordObject");
        }

        public override void Write(Utf8JsonWriter writer, HeadwordObject value, JsonSerializerOptions options)
        {
            if (value.HeadwordElementArray != null)
            {
                JsonSerializer.Serialize(writer, value.HeadwordElementArray, options);
                return;
            }
            if (value.Headword != null)
            {
                JsonSerializer.Serialize(writer, value.Headword, options);
                return;
            }

            throw new JsonException("Cannot marshal type HeadwordObject");
        }
    }
}

