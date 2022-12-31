﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M3USync.Models.Helpers
{
    public class SerieHelper
    {
        private static string[] RegexSeries = { @"(?<show>.+?)\s*S(?<season>\d{2})[Ee](?<episode>\d{2})", @"(?<show>.+?)\s*S(?<season>\d+)(?:\s*Épisode\s*|[Ee])?(?<episode>\d+)", @"(?<show>[^S\d{2}]*)(?<season>S)(?<season>\d{2}).*(?<episode>E)(?<episode>\d{2})" };

        /// <summary>
        /// Extracts the season and episode numbers.
        /// </summary>
        /// <param name="tvgName">Name of the TVG.</param>
        /// <returns></returns>
        public static (string show, int seasonNumber, int episodeNumber) ExtractSeasonAndEpisodeNumbers(string tvgName)
        {
            foreach (var regex in RegexSeries)
            {
                Match match = Regex.Match(tvgName, regex, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string show = match.Groups["show"].Value;
                    string season = match.Groups["season"].Value;
                    string episode = match.Groups["episode"].Value;

                    if (season.Length == 2 && episode.Length >= 1)
                    {
                        return (show, int.Parse(season), int.Parse(episode));
                    }
                }
            }
            return (string.Empty, 0, 0);
        }
    }
}
