﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Parsers.Langs
{
    public static class IsoLanguages
    {
        private static readonly HashSet<IsoLanguage> All = new HashSet<IsoLanguage>
                                                           {
                                                               new IsoLanguage("en", "", "eng", "English", Language.English),
                                                               new IsoLanguage("fr", "fr", "fra", "French", Language.French),
                                                               new IsoLanguage("es", "", "spa", "Spanish", Language.Spanish),
                                                               new IsoLanguage("de", "de", "deu", "German", Language.German),
                                                               new IsoLanguage("it", "", "ita", "Italian", Language.Italian),
                                                               new IsoLanguage("da", "", "dan", "Danish", Language.Danish),
                                                               new IsoLanguage("nl", "", "nld", "Dutch", Language.Dutch),
                                                               new IsoLanguage("ja", "", "jpn", "Japanese", Language.Japanese),
                                                               new IsoLanguage("is", "", "isl", "Icelandic", Language.Icelandic),
                                                               new IsoLanguage("zh", "cn", "zho", "Chinese", Language.Chinese),
                                                               new IsoLanguage("ru", "", "rus", "Russian", Language.Russian),
                                                               new IsoLanguage("pl", "", "pol", "Polish", Language.Polish),
                                                               new IsoLanguage("vi", "", "vie", "Vietnamese", Language.Vietnamese),
                                                               new IsoLanguage("sv", "", "swe", "Swedish", Language.Swedish),
                                                               new IsoLanguage("no", "", "nor", "Norwegian", Language.Norwegian),
                                                               new IsoLanguage("nb", "", "nob", "Norwegian Bokmal", Language.Norwegian),
                                                               new IsoLanguage("fi", "", "fin", "Finnish", Language.Finnish),
                                                               new IsoLanguage("tr", "", "tur", "Turkish", Language.Turkish),
                                                               new IsoLanguage("pt", "pt", "por", "Portuguese", Language.Portuguese),
                                                               new IsoLanguage("el", "", "ell", "Greek", Language.Greek),
                                                               new IsoLanguage("ko", "", "kor", "Korean", Language.Korean),
                                                               new IsoLanguage("hu", "", "hun", "Hungarian", Language.Hungarian),
                                                               new IsoLanguage("he", "", "heb", "Hebrew", Language.Hebrew),
                                                               new IsoLanguage("cs", "", "ces", "Czech", Language.Czech),
                                                               new IsoLanguage("hi", "", "hin", "Hindi", Language.Hindi),
                                                               new IsoLanguage("th", "", "tha", "Thai", Language.Thai),
                                                               new IsoLanguage("bg", "", "bul", "Bulgarian", Language.Bulgarian),
                                                               new IsoLanguage("ro", "", "ron", "Romanian", Language.Romanian),
                                                               new IsoLanguage("pt", "br", "", "Portuguese (Brazil)", Language.PortugueseBR),
                                                               new IsoLanguage("ar", "", "ara", "Arabic", Language.Arabic),
                                                               new IsoLanguage("uk", "", "ukr", "Ukrainian", Language.Ukrainian),
                                                               new IsoLanguage("fa", "", "fas", "Persian", Language.Persian),
                                                               new IsoLanguage("be", "", "ben", "Bengali", Language.Bengali),
                                                               new IsoLanguage("lt", "", "lit", "Lithuanian", Language.Lithuanian),
                                                               new IsoLanguage("sk", "", "slk", "Slovak", Language.Slovak),
                                                               new IsoLanguage("lv", "", "lav", "Latvian", Language.Latvian),
                                                               new IsoLanguage("es", "mx", "spa", "Spanish (Latino)", Language.SpanishLatino),
                                                               new IsoLanguage("ca", "", "cat", "Catalan", Language.Catalan),
                                                               new IsoLanguage("hr", "", "hrv", "Croatian", Language.Croatian),
                                                               new IsoLanguage("sr", "", "srp", "Serbian", Language.Serbian),
                                                               new IsoLanguage("bs", "", "bos", "Bosnian", Language.Bosnian),
                                                               new IsoLanguage("et", "", "est", "Estonian", Language.Estonian),
                                                               new IsoLanguage("ta", "", "tam", "Tamil", Language.Tamil),
                                                               new IsoLanguage("id", "", "ind", "Indonesian", Language.Indonesian)
                                                           };

        public static IsoLanguage Find(string isoCode)
        {
            var isoArray = isoCode.Split('-');

            var langCode = isoArray[0].ToLower();

            if (langCode.Length == 2)
            {
                // Lookup ISO639-1 code
                var isoLanguages = All.Where(l => l.TwoLetterCode == langCode).ToList();

                if (isoArray.Length > 1)
                {
                    isoLanguages = isoLanguages.Any(l => l.CountryCode == isoArray[1].ToLower()) ?
                        isoLanguages.Where(l => l.CountryCode == isoArray[1].ToLower()).ToList() : isoLanguages.Where(l => string.IsNullOrEmpty(l.CountryCode)).ToList();
                }

                return isoLanguages.FirstOrDefault();
            }
            else if (langCode.Length == 3)
            {
                // Lookup ISO639-2T code
                if (Iso639BTMap.TryGetValue(langCode, out var mapped))
                {
                    langCode = mapped;
                }

                return All.FirstOrDefault(l => l.ThreeLetterCode == langCode);
            }

            return null;
        }

        public static readonly ImmutableDictionary<string, string> Iso639BTMap = new Dictionary<string, string>
        {
            { "alb", "sqi" },
            { "arm", "hye" },
            { "baq", "eus" },
            { "bur", "mya" },
            { "chi", "zho" },
            { "cze", "ces" },
            { "dut", "nld" },
            { "fre", "fra" },
            { "geo", "kat" },
            { "ger", "deu" },
            { "gre", "ell" },
            { "ice", "isl" },
            { "mac", "mkd" },
            { "mao", "mri" },
            { "may", "msa" },
            { "per", "fas" },
            { "rum", "ron" },
            { "slo", "slk" },
            { "tib", "bod" },
            { "wel", "cym" }
        }.ToImmutableDictionary();

        public static IsoLanguage FindByName(string name)
        {
            return All.FirstOrDefault(l => l.EnglishName == name.Trim());
        }

        public static IsoLanguage Get(Language language)
        {
            return All.FirstOrDefault(l => l.Language == language);
        }
    }
}