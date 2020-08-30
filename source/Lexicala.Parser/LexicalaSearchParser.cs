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

        public async Task<SearchResultModel> SearchAsync(string searchText, string sourceLanguage, params string[] targetLanguages)
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
                var resultModel = ParseEntry(entry);
                returnModel.Results.Add(resultModel);
            }

            returnModel.Results = returnModel.Results.OrderBy(r => r.Text).ToList();
            return returnModel;
        }

        public async Task<SearchResultEntry> GetEntryAsync(string entryId, params string[] targetLanguages)
        {
            var entry = await _lexicalaClient.GetEntryAsync(entryId);
            return ParseEntry(entry, targetLanguages);
        }

        private static SearchResultEntry ParseEntry(Entry entry, params string [] targetLanguages)
        {
            var headwordElement = entry.Headword.HeadwordElementArray ?? new[] {entry.Headword.Headword};
            var pronunciations = new List<string>();
            var partOfSpeeches = new List<string>();
            foreach (var headword in headwordElement)
            {
                var pronElem = headword.Pronunciation.PronunciationArray ?? new[] {headword.Pronunciation.Pronunciation};
                foreach (var pronunciation in pronElem)
                {
                    if (pronunciation != null)
                    {
                        pronunciations.Add(pronunciation.Value);
                    }
                }

                var posElem = headword.Pos.PartOfSpeechArray ?? new[] {headword.Pos.PartOfSpeech};
                partOfSpeeches.AddRange(posElem.Where(pos => pos != null));
            }

            var resultModel = new SearchResultEntry
            {
                ETag = entry.Metadata.ETag,
                Id = entry.Id,
                Pos =string.Join(",", partOfSpeeches),
                SubCategory = headwordElement[0].Subcategorization,
                Pronunciation = string.Join(",", pronunciations),
                Text = string.Join("/", headwordElement.Select(hw => hw.Text)),
                Gender = headwordElement[0].Gender,
                Stems = headwordElement.SelectMany(hw => hw.AdditionalInflections).ToList()
            };

            foreach (var infl in headwordElement.Select(hw => hw.Inflections))
            {
                if (infl != null)
                {
                    foreach (var inflection in infl)
                    {
                        resultModel.Inflections.Add(new Dto.Inflection {Number = inflection.Number, Text = inflection.Text});
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
                    var translations =new List<Translation>();
                    if (targetLanguages?.Length > 0)
                    {
                        foreach (var languageCode in targetLanguages)
                        {
                            if (sourceSense.Translations.ContainsKey(languageCode))
                            {
                                translations.AddRange(ParseTranslation(languageCode, sourceSense.Translations[languageCode]));
                            }
                        }
                    }
                    else
                    {
                        foreach (var sourceSenseTranslation in sourceSense.Translations)
                        {
                            translations.AddRange(ParseTranslation(sourceSenseTranslation.Key, sourceSenseTranslation.Value));
                        }
                    }

                    targetSense.Translations = translations;
                }

                foreach (var sourceExample in sourceSense.Examples)
                {
                    var translations = new List<Translation>();

                    var example = new Dto.Example
                    {
                        Sentence = sourceExample.Text
                    };

                    if (sourceExample.Translations != null)
                    {
                        if (targetLanguages?.Length > 0)
                        {
                            foreach (var languageCode in targetLanguages)
                            {
                                if (sourceExample.Translations.ContainsKey(languageCode))
                                {
                                    translations.AddRange(ParseTranslation(languageCode, sourceExample.Translations[languageCode]));
                                }
                            }
                        }
                        else
                        {
                            foreach (var sourceExampleTranslation in sourceExample.Translations)
                            {
                                translations.AddRange(ParseTranslation(sourceExampleTranslation.Key,
                                    sourceExampleTranslation.Value));
                            }
                        }
                    }

                    example.Translations = translations;

                    targetSense.Examples.Add(example);
                }

                foreach (var compositionalPhrase in sourceSense.CompositionalPhrases)
                {
                        
                }

                resultModel.Senses.Add(targetSense);
            }

            return resultModel;
        }

        private static List<Translation> ParseTranslation(string languageCode, LanguageObject clo)
        {
            // json response is a bit flawed: it returns an object for 1 result, or an array for multiple results. this is difficult to deserialize so that's why this line looks a bit strange
            var translations = (clo.Language != null
                                   ? new List<Translation> {new Translation {Language = languageCode, Text = clo.Language.Text}}
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