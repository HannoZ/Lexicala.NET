using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Translation;
using Sense = Lexicala.NET.Response.Entries.Sense;

namespace Lexicala.NET.Internal
{
    /// <summary>
    /// Enriches lite entry/sense payloads with translations fetched from translation endpoints.
    /// </summary>
    internal sealed class TranslationEnricher
    {
        public async Task EnrichEntryTranslationsAsync(
            Entry entry,
            string targetLanguage,
            Func<string, string, string, CancellationToken, Task<TranslationResponse>> translateToAsync,
            Func<string, string, string, CancellationToken, Task<TranslationResponse>> translateExampleAsync,
            CancellationToken cancellationToken)
        {
            if (entry?.Senses is null || entry.Senses.Length == 0)
            {
                return;
            }

            var sourceText = entry.Headwords.FirstOrDefault()?.Text;
            var sourceLanguage = entry.Language;

            foreach (var sense in entry.Senses)
            {
                await EnrichSenseTranslationsAsync(
                    sense,
                    sourceText,
                    sourceLanguage,
                    targetLanguage,
                    translateToAsync,
                    translateExampleAsync,
                    cancellationToken);
            }
        }

        public async Task EnrichSenseTranslationsAsync(
            Sense sense,
            string sourceText,
            string sourceLanguage,
            string targetLanguage,
            Func<string, string, string, CancellationToken, Task<TranslationResponse>> translateToAsync,
            Func<string, string, string, CancellationToken, Task<TranslationResponse>> translateExampleAsync,
            CancellationToken cancellationToken)
        {
            if (sense is null)
            {
                return;
            }

            if (!HasTranslationForTarget(sense.Translations, targetLanguage) && !string.IsNullOrWhiteSpace(sourceText))
            {
                var translation = await translateToAsync(sourceText, targetLanguage, sourceLanguage, cancellationToken);
                var translatedText = TryExtractBestTranslationText(translation, sourceText);
                if (!string.IsNullOrWhiteSpace(translatedText))
                {
                    sense.Translations ??= [];
                    sense.Translations[targetLanguage] = new TranslationObject { Translation = new Translation { Text = translatedText } };
                }
            }

            if (sense.Examples is null || sense.Examples.Length == 0)
            {
                return;
            }

            foreach (var example in sense.Examples)
            {
                if (string.IsNullOrWhiteSpace(example?.Text) || HasTranslationForTarget(example.Translations, targetLanguage))
                {
                    continue;
                }

                var translation = await translateExampleAsync(example.Text, targetLanguage, sourceLanguage, cancellationToken);
                var translatedText = TryExtractBestTranslationText(translation, example.Text);
                if (string.IsNullOrWhiteSpace(translatedText))
                {
                    continue;
                }

                example.Translations ??= [];
                example.Translations[targetLanguage] = new TranslationObject { Translation = new Translation { Text = translatedText } };
            }
        }

        private static bool HasTranslationForTarget(Dictionary<string, TranslationObject> translations, string targetLanguage)
        {
            if (translations is null || !translations.TryGetValue(targetLanguage, out var translationObject))
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(translationObject.Translation?.Text)
                || translationObject.Translations?.Any(t => !string.IsNullOrWhiteSpace(t.Text)) == true;
        }

        private static string TryExtractBestTranslationText(TranslationResponse response, string sourceText)
        {
            if (response?.Results is null)
            {
                return null;
            }

            foreach (var result in response.Results)
            {
                var candidate = TryExtractTranslationText(result, sourceText);
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string TryExtractTranslationText(JsonElement element, string sourceText)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
                {
                    var text = textElement.GetString();
                    if (!string.IsNullOrWhiteSpace(text) && !string.Equals(text, sourceText, StringComparison.OrdinalIgnoreCase))
                    {
                        return text;
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    var nested = TryExtractTranslationText(property.Value, sourceText);
                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var nested = TryExtractTranslationText(item, sourceText);
                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }

            return null;
        }
    }
}
