using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Entries.JsonConverters;
using Lexicala.NET.Response.Search;

namespace Lexicala.NET
{
    internal sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('_');
                    }
                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }

    internal static class JsonSerializerDefaults
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false),
                new TranslationObjectConverter(),
                new Lexicala.NET.Response.Entries.JsonConverters.HeadwordObjectConverter(),
                new PronunciationObjectConverter(),
                new PosConverter(),
                new AlternativeScriptsObjectConverter(),
                new FlexibleIntConverter(),
                new Lexicala.NET.Response.Search.HeadwordObjectConverter()
            }
        };
    }
}
