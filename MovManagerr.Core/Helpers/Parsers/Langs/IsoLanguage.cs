﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Parsers.Langs
{
    public class IsoLanguage
    {
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public string CountryCode { get; set; }
        public string EnglishName { get; set; }
        public Language Language { get; set; }

        public IsoLanguage(string twoLetterCode, string countryCode, string threeLetterCode, string englishName, Language language)
        {
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
            CountryCode = countryCode;
            EnglishName = englishName;
            Language = language;
        }
    }
}