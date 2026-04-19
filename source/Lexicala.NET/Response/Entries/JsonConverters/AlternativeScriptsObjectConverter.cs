using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class AlternativeScriptsObjectConverter : JsonConverter<AlternativeScriptsObject>
    {
        public override AlternativeScriptsObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    var objectValue = JsonSerializer.Deserialize<AlternativeScripts>(ref reader, options);
                    return new AlternativeScriptsObject { AlternativeScripts = objectValue };
                case JsonTokenType.StartArray:
                    var arrayValue = JsonSerializer.Deserialize<AlternativeScripts[]>(ref reader, options);
                    return new AlternativeScriptsObject { AlternativeScriptsArray = arrayValue };
            }

            throw new JsonException("Cannot unmarshal type AlternativeScriptsObject");
        }

        public override void Write(Utf8JsonWriter writer, AlternativeScriptsObject value, JsonSerializerOptions options)
        {
            if (value.AlternativeScriptsArray != null)
            {
                JsonSerializer.Serialize(writer, value.AlternativeScriptsArray, options);
                return;
            }
            if (value.AlternativeScripts != null)
            {
                JsonSerializer.Serialize(writer, value.AlternativeScripts, options);
                return;
            }

            throw new JsonException("Cannot marshal type AlternativeScriptsObject");
        }
    }
}

