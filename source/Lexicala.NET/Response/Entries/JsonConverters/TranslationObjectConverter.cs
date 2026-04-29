using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class TranslationObjectConverter : JsonConverter<TranslationObject>
    {
        public override TranslationObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<Translation>(ref reader, options);
                    return new TranslationObject { Translation = objectValue };
                case JsonTokenType.StartArray:
                    var arrayValue = JsonSerializer.Deserialize<Translation[]>(ref reader, options);
                    return new TranslationObject { Translations = arrayValue };
            }

            throw new JsonException("Cannot unmarshal type TranslationObject");
        }

        public override void Write(Utf8JsonWriter writer, TranslationObject value, JsonSerializerOptions options)
        {
            if (value.Translations != null)
            {
                JsonSerializer.Serialize(writer, value.Translations, options);
                return;
            }
            if (value.Translation != null)
            {
                JsonSerializer.Serialize(writer, value.Translation, options);
                return;
            }

            throw new JsonException("Cannot marshal type TranslationObject");
        }
    }
}

