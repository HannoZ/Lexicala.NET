using Lexicala.NET.Client;
using Lexicala.NET.Client.Response.Entries;
using Lexicala.NET.Client.Response.Languages;
using Lexicala.NET.Parser.Dto;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sense = Lexicala.NET.Parser.Dto.Sense;

namespace Lexicala.NET.Parser
{
    public class LexicalaSearchParser : ILexicalaSearchParser
    {
        private readonly ILexicalaClient _lexicalaClient;
        private readonly IMemoryCache _memoryCache;

        public LexicalaSearchParser(ILexicalaClient lexicalaClient, IMemoryCache memoryCache)
        {
            _lexicalaClient = lexicalaClient;
            _memoryCache = memoryCache;
        }

        public async Task<SearchResultModel> SearchAsync(string searchText, string sourceLanguage)
        {
            var languages = await LoadLanguages();
            if (!languages.Global.SourceLanguages.Contains(sourceLanguage))
            {
                throw new ArgumentException($"Invalid value. '{sourceLanguage}' does not appear in the Global source languages list", nameof(sourceLanguage));
            }

            var searchResult = await _lexicalaClient.BasicSearchAsync(searchText.ToLowerInvariant(), sourceLanguage: sourceLanguage);
            
            var entries = new List<Entry>();
            foreach (var result in searchResult.Results)
            {
                if (entries.Any(e => e.Id == result.Id))
                {
                    continue;
                }

                var entry = await _lexicalaClient.GetEntryAsync(result.Id);
                entries.Add(entry);
                foreach (var relatedEntry in entry.RelatedEntries)
                {
                    if (entries.Any(e => e.Id == relatedEntry))
                    {
                        continue;
                    }

                    var related = await _lexicalaClient.GetEntryAsync(relatedEntry);
                    entries.Add(related);
                }
            }

            var returnModel = new SearchResultModel
            {
                SearchText = searchText.ToLowerInvariant(),
                ETag = searchResult.Metadata.ETag
            };

            foreach (var entry in entries)
            {
                var headwordElement = entry.Headword.HeadwordElementArray ?? new[] {entry.Headword.Headword};

                var resultModel = new SearchResultEntry
                {
                    ETag = entry.Metadata.ETag,
                    Id = entry.Id,
                    Pos = headwordElement[0].Pos,
                    SubCategory = headwordElement[0].Subcategorization,
                    Pronunciation = string.Join("/", headwordElement.Select(hw=>hw.Pronunciation?.Value)),
                    Text = string.Join("/", headwordElement.Select(hw=> hw.Text) ),
                    Gender = headwordElement[0].Gender,
                    Stems = headwordElement.SelectMany(hw=>hw.AdditionalInflections).ToList()
                };

                foreach (var infl in headwordElement.Select(hw => hw.Inflections))
                {
                    if (infl != null)
                    {
                        foreach (var inflection in infl)
                        {
                            resultModel.Inflections.Add(new Dto.Inflection{ Number = inflection.Number, Text = inflection.Text});
                        }
                    }
                }
                foreach (var sourceSense in entry.Senses)
                {
                    var targetSense = new Sense
                    {
                        Id = sourceSense.Id,
                        Definition = sourceSense.Definition,
                        Synonyms = sourceSense.Synonyms
                    };

                    if (sourceSense.Translations != null)
                    {
                        var translations = ParseTranslation(sourceSense.Translations.Br, "br");
                        translations.AddRange(ParseTranslation(sourceSense.Translations.Dk, "dk"));
                        translations.AddRange(ParseTranslation(sourceSense.Translations.En, "en"));
                        translations.AddRange(ParseTranslation(sourceSense.Translations.Fr, "fr"));
                        translations.AddRange(ParseTranslation(sourceSense.Translations.Nl, "nl"));
                        translations.AddRange(ParseTranslation(sourceSense.Translations.No, "no"));
                        translations.AddRange(ParseTranslation(sourceSense.Translations.Sv, "sv"));

                        targetSense.Translations = translations;
                    }

                    foreach (var sourceExample in sourceSense.Examples)
                    {
                        var example = new Dto.Example
                        {
                            Sentence = sourceExample.Text
                        };

                        AddExampleTranslationIfExists(sourceExample.Translations?.Br, "br", example);
                        AddExampleTranslationIfExists(sourceExample.Translations?.Dk, "dk", example);
                        AddExampleTranslationIfExists(sourceExample.Translations?.En, "en", example);
                        AddExampleTranslationIfExists(sourceExample.Translations?.No, "no", example);
                        AddExampleTranslationIfExists(sourceExample.Translations?.Nl, "nl", example);
                        AddExampleTranslationIfExists(sourceExample.Translations?.Sv, "sv", example);

                        targetSense.Examples.Add(example);
                    }

                    resultModel.Senses.Add(targetSense);
                }

                returnModel.Results.Add(resultModel);
            }

            returnModel.Results = returnModel.Results.OrderBy(r => r.Text).ToList();
            return returnModel;
        }

        private static void AddExampleTranslationIfExists(CommonLanguage translation, string languageCode, Dto.Example example)
        {
            var text = translation?.Text;
            if (text != null)
            {
                example.Translations.Add(new Translation
                {
                    Language = languageCode,
                    Text = text
                });
            }
        }

        private static List<Translation> ParseTranslation(CommonLanguageObject clo, string languageCode)
        {
            // json response is a bit flawed: it returns an object for 1 result, or an array for multiple results. this is difficult to deserialize so that's why this line looks a bit strange
            var translations = (clo.CommonLanguage != null
                                   ? new List<Translation> {new Translation {Language = languageCode, Text = clo.CommonLanguage.Text}}
                                   : clo.CommonLanguageObjectArray?.Select(nl => new Translation {Text = nl.Text, Language = languageCode}).ToList())
                               ?? new List<Translation>();
            return translations;
        }

        private async Task<Resources> LoadLanguages()
        {
            if (!_memoryCache.TryGetValue("languages", out Resources languages))
            {
                languages = (await _lexicalaClient.LanguagesAsync()).Resources;

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));
                cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                // Save data in cache.
                _memoryCache.Set("languages", languages, cacheEntryOptions);
            }

            return languages;
        }
    }
}