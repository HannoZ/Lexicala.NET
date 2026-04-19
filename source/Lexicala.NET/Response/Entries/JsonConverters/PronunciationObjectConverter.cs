using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class PronunciationObjectConverter : JsonConverter<PronunciationObject>
    {
        public override PronunciationObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<Pronunciation>(ref reader, options);
                    return new PronunciationObject { Pronunciation = objectValue };
                case JsonTokenType.StartArray:
                    var arrayValue = JsonSerializer.Deserialize<Pronunciation[]>(ref reader, options);
                    return new PronunciationObject { PronunciationArray = arrayValue };
            }

            throw new JsonException("Cannot unmarshal type PronunciationObject");
        }

        public override void Write(Utf8JsonWriter writer, PronunciationObject value, JsonSerializerOptions options)
        {
            if (value.PronunciationArray != null)
            {
                JsonSerializer.Serialize(writer, value.PronunciationArray, options);
                return;
            }
            if (value.Pronunciation != null)
            {
                JsonSerializer.Serialize(writer, value.Pronunciation, options);
                return;
            }

            throw new JsonException("Cannot marshal type PronunciationObject");
        }
    }
}

