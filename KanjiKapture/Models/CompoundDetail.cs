using System;
using System.Collections.Generic;

namespace KanjiKapture.Models
{
    public class CompoundDetail
    {
        public required string Slug { get; set; }
        public bool? IsCommon { get; set; }
        public List<string> JlptLevels { get; set; } = [];
        public List<KanjiJapanese> Japanese { get; set; } = [];
        public List<Sense> Senses { get; set; } = [];
        public Attribution? Attribution { get; set; }
    }

    public class KanjiJapanese
    {
        public required string Word { get; set; }
        public required string Reading { get; set; }
    }

    public class Sense
    {
        public List<string> EnglishDefinitions { get; set; } = [];
        public List<string> PartsOfSpeech { get; set; } = [];
        public List<Link> Links { get; set; } = [];
        public List<string> Tags { get; set; } = [];
        public List<string> Restrictions { get; set; } = [];
        public List<string> SeeAlso { get; set; } = [];
        public List<string> Antonyms { get; set; } = [];
        public List<string> Source { get; set; } = [];
        public List<string> Info { get; set; } = [];
        public List<Sentence> Sentences { get; set; } = [];
    }

    public class Link
    {
        public string? Text { get; set; }
        public string? Url { get; set; }
    }

    public class Sentence
    {
        public string? Japanese { get; set; }
        public string? English { get; set; }
    }

    public class Attribution
    {
        public bool? Jmdict { get; set; }
        public bool? Jmnedict { get; set; }
        public string? Dbpedia { get; set; }
    }
}
