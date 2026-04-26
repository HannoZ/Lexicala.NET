using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lexicala.NET.Parsing.Dto;
using Lexicala.NET.Request;
using Lexicala.NET.Response.Entries;
using Lexicala.NET.Response.Languages;
using Lexicala.NET.Response.Search;
using Microsoft.Extensions.Caching.Memory;
using Translation = Lexicala.NET.Parsing.Dto.Translation;

namespace Lexicala.NET.Parsing
{
    /// <inheritdoc />
    public class LexicalaSearchParser : ILexicalaSearchParser
    {
        private readonly ILexicalaClient _lexicalaClient;
        private readonly IMemoryCache _memoryCache;
        
        /// <summary>
        /// Creates a new instance of the <see cref="LexicalaSearchParser"/> class.
        /// </summary>
        /// <remarks>Intended to be used by the current dependency injection framework.</remarks>
        public LexicalaSearchParser(ILexicalaClient lexicalaClient, IMemoryCache memoryCache)
        {
            _lexicalaClient = lexicalaClient;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc />
        public async Task<SearchResultModel> SearchAsync(string searchText, string sourceLanguage, params string[] targetLanguages)
        {
            ArgumentException.ThrowIfNullOrEmpty(searchText, nameof(searchText));
            ArgumentException.ThrowIfNullOrEmpty(sourceLanguage, nameof(sourceLanguage));

            var languages = await LoadLanguages();
            if (!languages.Global.SourceLanguages.Contains(sourceLanguage))
            {
                throw new ArgumentException($"Invalid value. '{sourceLanguage}' does not appear in the Global source languages list", nameof(sourceLanguage));
            }

            var searchResult = await _lexicalaClient.BasicSearchAsync(searchText.ToLowerInvariant(), sourceLanguage);

            return await ProcessSearchResult(searchText, searchResult, targetLanguages);
        }


        /// <inheritdoc />
        public async Task<SearchResultModel> SearchAsync(AdvancedSearchRequest searchRequest, params string[] targetLanguages)
        {
            ArgumentNullException.ThrowIfNull(searchRequest);
            ArgumentException.ThrowIfNullOrEmpty(searchRequest.Language, nameof(searchRequest.Language));
            ArgumentException.ThrowIfNullOrEmpty(searchRequest.SearchText, nameof(searchRequest.SearchText));

            var languages = await LoadLanguages();
            if (!languages.Global.SourceLanguages.Contains(searchRequest.Language))
            {
                throw new ArgumentException($"Invalid value. '{searchRequest.Language}' does not appear in the Global source languages list", nameof(searchRequest.Language));
            }

            var searchResult = await _lexicalaClient.AdvancedSearchAsync(searchRequest);
            return await ProcessSearchResult(searchRequest.SearchText, searchResult, targetLanguages);
        }

        /// <inheritdoc />
        public async Task<SearchResultEntry> GetEntryAsync(string entryId, params string[] targetLanguages)
        {
            var entry = await _lexicalaClient.GetEntryAsync(entryId);
            return ParseEntry(entry, targetLanguages);
        }


        private async Task<SearchResultModel> ProcessSearchResult(string searchText, SearchResponse searchResult, string[] targetLanguages)
        {
            // Collect all unique entry IDs to fetch
            var allIds = new HashSet<string>();
            foreach (var result in searchResult.Results)
            {
                allIds.Add(result.Id);
            }

            // Fetch initial entries in parallel
            var initialEntryTasks = allIds.Select(id => _lexicalaClient.GetEntryAsync(id)).ToArray();
            var initialEntries = await Task.WhenAll(initialEntryTasks);

            // Collect related entry IDs
            var relatedIds = new HashSet<string>();
            foreach (var entry in initialEntries)
            {
                if (entry.RelatedEntries != null)
                {
                    foreach (var relatedId in entry.RelatedEntries)
                    {
                        if (!allIds.Contains(relatedId))
                        {
                            relatedIds.Add(relatedId);
                            allIds.Add(relatedId);
                        }
                    }
                }
            }

            // Fetch related entries in parallel
            var relatedEntryTasks = relatedIds.Select(id => _lexicalaClient.GetEntryAsync(id)).ToArray();
            var relatedEntries = await Task.WhenAll(relatedEntryTasks);

            // Combine all entries
            var entries = initialEntries.Concat(relatedEntries).ToList();

            var returnModel = new SearchResultModel
            {
                SearchText = searchText.ToLowerInvariant(),
                TotalResults = searchResult.NResults,
                Metadata = searchResult.Metadata
            };

            foreach (var entry in entries)
            {
                var resultModel = ParseEntry(entry, targetLanguages);
                returnModel.Results.Add(resultModel);
            }

            returnModel.Results = returnModel.Results.OrderBy(r => r.Text).ToList();
            return returnModel;
        }


        private static SearchResultEntry ParseEntry(Entry entry, params string[] targetLanguages)
        {
            // Extract all pronunciations from all headwords into a flat collection
            // Entry can have multiple headwords, each with multiple pronunciations
            var pronunciations = new List<string>();
            foreach (var headword in entry.Headwords)
            {
                foreach (var pronunciation in headword.Pronunciations)
                {
                    if (pronunciation != null)
                    {
                        pronunciations.Add(pronunciation.Value);
                    }
                }
            }

            // Extract gender from first headword that has one
            // Gender is a property that may vary across different headword entries
            string gender = null;
            foreach (var headword in entry.Headwords)
            {
                gender = headword.Gender;
                if (gender != null) break;  // Use first available gender
            }

            // Collect all unique parts of speech from all headwords
            var pos = entry.Headwords.SelectMany(hw => hw.PartOfSpeeches).Distinct().ToList();

            // Build result model with aggregated data from all headwords
            var resultModel = new SearchResultEntry
            {
                ETag = entry.Metadata.ETag,
                Id = entry.Id,
                Pos = string.Join(",", pos),  // CSV format for multiple parts of speech
                SubCategory = string.Join(",", entry.Headwords.Select(hw => hw.Subcategorization)),
                Pronunciation = string.Join(",", pronunciations),  // All pronunciations concatenated
                Text = string.Join("/", entry.Headwords.Select(hw => hw.Text)),  // Multiple headwords separated by /
                Gender = gender
            };

            // Add any additional inflectional stems from headwords
            foreach (var stem in entry.Headwords.SelectMany(hw => hw.AdditionalInflections))
            {
                resultModel.Stems.Add(stem);
            }

            // Extract and add all inflections from all headwords
            foreach (var infl in entry.Headwords.Select(hw => hw.Inflections))
            {
                if (infl != null)
                {
                    foreach (var inflection in infl)
                    {
                        resultModel.Inflections.Add(new Dto.Inflection { Number = inflection.Number, Text = inflection.Text });
                    }
                }
            }

            // Parse senses with translation filtering applied
            foreach (var sourceSense in entry.Senses)
            {
                var targetSense = ParseSense(sourceSense, targetLanguages);
                resultModel.Senses.Add(targetSense);
            }

            return resultModel;
        }

        private static Dto.Sense ParseSense(Response.Entries.Sense sourceSense, string[] targetLanguages)
        {
            var targetSense = new Dto.Sense
            {
                Id = sourceSense.Id,
                Definition = sourceSense.Definition
            };

            foreach (var synonym in sourceSense.Synonyms)
            {
                targetSense.Synonyms.Add(synonym);
            }

            if (sourceSense.Translations != null)
            {
                foreach (var translation in FilterTranslations(sourceSense.Translations, targetLanguages))
                {
                    targetSense.Translations.Add(translation);
                }
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
                    translations.AddRange(FilterTranslations(sourceExample.Translations, targetLanguages));
                }

                foreach (var translation in translations)
                {
                    example.Translations.Add(translation);
                }
                targetSense.Examples.Add(example);
            }

            foreach (var compositionalPhrase in sourceSense.CompositionalPhrases)
            {
                var comp = new Dto.CompositionalPhrase
                {
                    Text = compositionalPhrase.Text,
                    Definition = compositionalPhrase.Definition
                };

                if (compositionalPhrase.Translations != null)
                {
                    foreach (var translation in FilterTranslations(compositionalPhrase.Translations, targetLanguages))
                    {
                        comp.Translations.Add(translation);
                    }
                }

                foreach (var sourceExample in compositionalPhrase.Examples)
                {
                    var translations = new List<Translation>();

                    var example = new Dto.Example
                    {
                        Sentence = sourceExample.Text
                    };

                    if (sourceExample.Translations != null)
                    {
                        translations.AddRange(FilterTranslations(sourceExample.Translations, targetLanguages));
                    }

                    foreach (var translation in translations)
                    {
                        example.Translations.Add(translation);
                    }
                    comp.Examples.Add(example);
                }

                foreach (var sense in compositionalPhrase.Senses)
                {
                    var subSense = ParseSense(sense, targetLanguages);
                    comp.Senses.Add(subSense);
                }

                targetSense.CompositionalPhrases.Add(comp);               
            }

            return targetSense;
        }

        private static List<Translation> ParseTranslation(string languageCode, TranslationObject clo)
        {
            // json response is a bit flawed: it returns an object for 1 result, or an array for multiple results. this is difficult to deserialize so that's why this line looks a bit strange
            var translations = (clo.Translation != null
                                   ? [new Translation { Language = languageCode, Text = clo.Translation.Text }]
                                   : clo.Translations?.Select(nl => new Translation { Text = nl.Text, Language = languageCode }).ToList())
                               ?? [];
            return translations;
        }

        private static List<Translation> FilterTranslations(Dictionary<string, TranslationObject> translationsDict, string[] targetLanguages)
        {
            if (targetLanguages?.Length > 0)
            {
                return [.. targetLanguages
                    .Where(translationsDict.ContainsKey)
                    .SelectMany(languageCode => ParseTranslation(languageCode, translationsDict[languageCode]))];
            }

            return [.. translationsDict.SelectMany(kvp => ParseTranslation(kvp.Key, kvp.Value))];
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