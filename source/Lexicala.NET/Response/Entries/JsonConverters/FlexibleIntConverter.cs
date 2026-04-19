using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class FlexibleIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intValue))
            {
                return intValue;
            }

            if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out var parsedValue))
            {
                return parsedValue;
            }

            throw new JsonException("Cannot convert JSON value to int.");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
