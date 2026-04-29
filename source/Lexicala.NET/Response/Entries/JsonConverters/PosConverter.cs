using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class PosConverter : JsonConverter<Pos>
    {
        public override Pos Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    return new Pos { PartOfSpeech = stringValue };
                case JsonTokenType.StartArray:
                    var arrayValue = JsonSerializer.Deserialize<string[]>(ref reader, options);
                    return new Pos { PartOfSpeechArray = arrayValue };
            }

            throw new JsonException("Cannot unmarshal type PartOfSpeech");
        }

        public override void Write(Utf8JsonWriter writer, Pos value, JsonSerializerOptions options)
        {
            if (value.PartOfSpeech != null)
            {
                JsonSerializer.Serialize(writer, value.PartOfSpeech, options);
                return;
            }
            if (value.PartOfSpeechArray != null)
            {
                JsonSerializer.Serialize(writer, value.PartOfSpeechArray, options);
                return;
            }

            throw new JsonException("Cannot marshal type PartOfSpeech");
        }
    }
}

