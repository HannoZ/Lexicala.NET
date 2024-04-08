using System;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Entries.JsonConverters
{
    internal class AlternativeScriptsObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(AlternativeScriptsObject) || t == typeof(AlternativeScriptsObject?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<AlternativeScripts>(reader);
                    return new AlternativeScriptsObject { AlternativeScripts = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<AlternativeScripts[]>(reader);
                    return new AlternativeScriptsObject { AlternativeScriptsArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type AlternativeScriptsObject");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (AlternativeScriptsObject)untypedValue;
            if (value.AlternativeScriptsArray != null)
            {
                serializer.Serialize(writer, value.AlternativeScriptsArray);
                return;
            }
            if (value.AlternativeScripts != null)
            {
                serializer.Serialize(writer, value.AlternativeScripts);
                return;
            }
            throw new Exception("Cannot marshal type AlternativeScriptsObject");
        }

        public static readonly AlternativeScriptsObjectConverter Singleton = new AlternativeScriptsObjectConverter();
    }
}