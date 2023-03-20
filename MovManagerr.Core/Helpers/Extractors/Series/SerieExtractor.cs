using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Infrastructures.Configurations;
using OpenAI_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Extractors.Series
{
    public class SerieExtractor
    {
        public SerieAnalyseResult ExtractSerie(string path)
        {
            var result = new SerieAnalyseResult();
            result.SeriePath = path;
            result.SerieName = Path.GetFileName(path);

            string[] extensions = { ".mkv", ".avi", ".mp4" };
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(file => extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (string file in files)
            {
                var episodeResult = new EpisodeAnalyseResult();
                episodeResult.EpisodePath = file;
                episodeResult.EpisodeName = Path.GetFileNameWithoutExtension(file);

                // Try to extract the season and episode numbers from the file name
                if (TryExtractSeasonAndEpisodeFromFileName(episodeResult.EpisodeName, out int season, out int episode))
                {
                    episodeResult.Season = season;
                    episodeResult.Episode = episode;
                    result.Episodes.Add(episodeResult);
                }
                // If the file name doesn't contain the season and episode numbers, try to extract them from the folder name
                else if (TryExtractSeasonAndEpisodeFromFolderName(file, out season, out episode))
                {
                    episodeResult.Season = season;
                    episodeResult.Episode = episode;
                    result.Episodes.Add(episodeResult);
                }
                else
                {
                    OpenAIAPI api = new OpenAIAPI(Preferences.Instance.Settings.OpenAIApiKey); // shorthand

                    // create a completion request with a prompt to extract the season and episode numbers
                    string prompt = $"Given a TV show episode file name '{episodeResult.EpisodePath}', please extract the season and episode numbers. Use the format example 'S01E01'.";

                    try
                    {
                        string? promptResult = Task.Run<string?>(async () => await api.Completions.GetCompletion(prompt)).Result;

                        if (!string.IsNullOrEmpty(promptResult) && Regex.IsMatch(promptResult, @"S\d{2}E\d{2}"))
                        {
                            Match match = Regex.Match(promptResult, @"S(?<season>\d{2})E(?<episode>\d{2})");
                            if (match.Success)
                            {
                                season = int.Parse(match.Groups["season"].Value);
                                episode = int.Parse(match.Groups["episode"].Value);
                                episodeResult.Season = season;
                                episodeResult.Episode = episode;
                                result.Episodes.Add(episodeResult);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error or handle it as needed
                    }

                    //process the result

                }
            }
            return result;

        }

        private bool TryExtractSeasonAndEpisodeFromFileName(string fileName, out int season, out int episode)
        {
            var regex = new Regex(@"[sS](\d{1,2})[eE](\d{1,2})");
            var match = regex.Match(fileName);
            if (match.Success)
            {
                season = int.Parse(match.Groups[1].Value);
                episode = int.Parse(match.Groups[2].Value);
                return true;
            }
            season = 0;
            episode = 0;
            return false;
        }

        private bool TryExtractSeasonAndEpisodeFromFolderName(string filePath, out int season, out int episode)
        {
            var regex = new Regex(@"[sS](\d{1,2})[eE](\d{1,2})");
            var folderName = Path.GetFileName(Path.GetDirectoryName(filePath));
            var match = regex.Match(folderName);
            if (match.Success)
            {
                season = int.Parse(match.Groups[1].Value);
                episode = int.Parse(match.Groups[2].Value);
                return true;
            }
            season = 0;
            episode = 0;
            return false;
        }



    }

    /// <summary>
    /// Pour les serie /{Nom de la serie}/{Saison}/{Episode} ou {*/*/episode}
    /// </summary>
    public class BasicExtraction
    {
        private readonly string Path;

        public BasicExtraction(string path)
        {
            Path = path;
        }

        //public SerieAnalyseResult Extract()
        //{
        //    var result = new SerieAnalyseResult();

        //    // Extraire le nom de la série à partir du chemin d'accès
        //    result.SeriePath = Path;
        //    string[] pathParts = Path.Split('/');
        //    result.SerieName = pathParts.Last();

        //    // Extraire les informations de chaque épisode
        //    string[] files = Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories);
        //    foreach (string file in files)
        //    {
        //        string fileName = Path.GetFileNameWithoutExtension(file);

        //        // Vérifier si le fichier est un épisode en utilisant le format {*/*/episode}
        //        if (fileName.ToLower().EndsWith("episode"))
        //        {
        //            EpisodeAnalyseResult episode = ExtractEpisode(fileName);
        //            episode.EpisodePath = file;
        //            result.Episodes.Add(episode);
        //        }
        //        else
        //        {
        //            // Extraire les informations de l'épisode en utilisant le format /{Nom de la serie}/{Saison}/{Episode}
        //            if (fileName.ToLower().StartsWith(result.SerieName.ToLower()))
        //            {
        //                string[] nameParts = fileName.Split(' ');
        //                if (nameParts.Length >= 3 && int.TryParse(nameParts[nameParts.Length - 1], out int episodeNumber))
        //                {
        //                    EpisodeAnalyseResult episode = ExtractEpisode(fileName);
        //                    episode.EpisodePath = file;
        //                    result.Episodes.Add(episode);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        private EpisodeAnalyseResult ExtractEpisode(string fileName)
        {
            EpisodeAnalyseResult episode = new EpisodeAnalyseResult();

            // Extraire le numéro de saison et d'épisode à partir du nom du fichier
            string[] nameParts = fileName.Split(' ');
            episode.Season = int.Parse(nameParts[nameParts.Length - 3].Replace("s", "").Replace("S", ""));
            episode.Episode = int.Parse(nameParts[nameParts.Length - 2].Replace("e", "").Replace("E", ""));
            episode.EpisodeName = string.Join(' ', nameParts.Take(nameParts.Length - 3));

            return episode;
        }
    }


    public class SerieAnalyseResult
    {

        public SerieAnalyseResult()
        {
            Episodes = new List<EpisodeAnalyseResult>();
        }

        public string SerieName { get; set; }
        public string SeriePath { get; set; }
        public List<EpisodeAnalyseResult> Episodes { get; set; }
    }

    public class EpisodeAnalyseResult
    {
        public string EpisodeName { get; set; }
        public string EpisodePath { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
    }
}

