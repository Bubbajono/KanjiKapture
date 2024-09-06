using KanjiKapture.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanjiKapture.Utilities
{
    public static class TextUtility
    {
        private static readonly HttpClient client = new();

        public static bool ContainsNonKanji(string text)
        {
            foreach (char c in text)
            {
                // Allow Kanji only (CJK Unified Ideographs: \u4E00-\u9FFF)
                if (!(c >= '\u4E00' && c <= '\u9FFF'))
                {
                    return true; // Found a non-Kanji character
                }
            }
            return false; // All characters are Kanji
        }

        public static async Task<JObject?> GetKanjiCompounds(string kanji)
        {
            string url = $"https://jisho.org/api/v1/search/words?keyword={kanji}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseBody);
        }

        public static string[] GetCompoundList(string search, JObject? json)
        {
            if (json is null) return [];

            if (json["data"] is not JArray dataArray)
            {
                // Handle the case where json["data"] is null
                return []; // or whatever default value makes sense
            }

            var compounds = dataArray
                .Where(data => data != null)
                .SelectMany(data => data["japanese"] ?? Enumerable.Empty<JToken>())
                .Select(japanese => japanese["word"] as JValue)
                .OfType<JValue>()
                .Select(v => v.ToString())
                .Distinct()
                .Where(x => !ContainsNonKanji(x) && x.Contains(search))
                .ToArray();

            return compounds;
        }

        public static CompoundDetail? GetCompoundDetail(JObject? json, string compound)
        {
            if (json == null || string.IsNullOrEmpty(compound)) return null;

            // Ensure json["data"] is a JArray
            if (json["data"] is not JArray dataArray)
            {
                return null;
            }

            // Search for the specific compound
            var compoundData = dataArray
                .FirstOrDefault(data => (string?)data["slug"] == compound);

            if (compoundData == null)
            {
                return null;
            }

            // Handle Attribution safely
            Attribution? attribution = null;
            var attributionData = compoundData["attribution"];
            if (attributionData != null)
            {
                attribution = new Attribution
                {
                    Jmdict = (bool?)attributionData["jmdict"] ?? false,
                    Jmnedict = (bool?)attributionData["jmnedict"] ?? false,
                    Dbpedia = (string?)attributionData["dbpedia"] ?? string.Empty
                };
            }

            // Create and populate CompoundDetail object
            var detail = new CompoundDetail
            {
                Slug = (string?)compoundData["slug"] ?? string.Empty,
                IsCommon = (bool?)compoundData["is_common"] ?? false,
                JlptLevels = compoundData["jlpt"]?.Select(jlpt => (string?)jlpt ?? string.Empty).ToList() ?? [],
                Japanese = compoundData["japanese"]?
                    .Select(jp => new KanjiJapanese
                    {
                        Word = (string?)jp["word"] ?? string.Empty,
                        Reading = (string?)jp["reading"] ?? string.Empty
                    }).ToList() ?? [],
                Senses = compoundData["senses"]?
                    .Select(sense => new Sense
                    {
                        EnglishDefinitions = sense["english_definitions"]?
                            .Select(def => (string?)def ?? string.Empty).ToList() ?? [],
                        PartsOfSpeech = sense["parts_of_speech"]?
                            .Select(pos => (string?)pos ?? string.Empty).ToList() ?? [],
                        Links = sense["links"]?
                            .Select(link => new Link
                            {
                                Text = (string?)link["text"] ?? string.Empty,
                                Url = (string?)link["url"] ?? string.Empty
                            }).ToList() ?? [],
                        Tags = sense["tags"]?
                            .Select(tag => (string?)tag ?? string.Empty).ToList() ?? [],
                        Restrictions = sense["restrictions"]?
                            .Select(r => (string?)r ?? string.Empty).ToList() ?? [],
                        SeeAlso = sense["see_also"]?
                            .Select(see => (string?)see ?? string.Empty).ToList() ?? [],
                        Antonyms = sense["antonyms"]?
                            .Select(ant => (string?)ant ?? string.Empty).ToList() ?? [],
                        Source = sense["source"]?
                            .Select(src => (string?)src ?? string.Empty).ToList() ?? [],
                        Info = sense["info"]?
                            .Select(info => (string?)info ?? string.Empty).ToList() ?? [],
                        Sentences = sense["sentences"]?
                            .Select(sentence => new Sentence
                            {
                                Japanese = (string?)sentence["japanese"] ?? string.Empty,
                                English = (string?)sentence["english"] ?? string.Empty
                            }).ToList() ?? []
                    }).ToList() ?? [],
                Attribution = attribution
            };

            return detail;
        }
    }
}