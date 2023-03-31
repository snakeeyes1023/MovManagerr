using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Helpers.Extensions;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Tmdb;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.RegularExpressions;

namespace MovManagerr.Core.Data
{
    [Table("tvshows")]
    public class TvShow
    {
        public string Name { get; set; }

        #region Tmdb

        public TMDbLib.Objects.TvShows.TvShow TvShowTmdb { get; private set; }
        public DateTime? LastSearchAttempt { get; set; }
        public int TmdbId { get; set; }
        #endregion

        public List<Episode> Episodes { get; protected set; }

        public DirectoryManager GetDirectoryManager()
        {
            return Preferences.Instance.Settings.GetContentPreference<TvShow>().GetDirectoryManager();
        }

        public string GetPath(bool createDirectory = true)
        {
            string title = Name;
            string year = "0000";
            
            if (TvShowTmdb != null)
            {
                title = TvShowTmdb?.GetValidName() ?? Name;
                year = TvShowTmdb != null && TvShowTmdb.FirstAirDate.HasValue ? TvShowTmdb.FirstAirDate.Value.Year.ToString() : "0000";
            }

            char[] invalidChars = Path.GetInvalidPathChars();
            var cleanedTitle = string.Join("_", title.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            // replace  by nothing in Regex.Replace(path, @"[ ]{2,}", " ");
            cleanedTitle = Regex.Replace(cleanedTitle, @"[ ]{2,}", " ").Replace(":", string.Empty);
            cleanedTitle = $"{cleanedTitle} ({year})";

            if (TvShowTmdb != null)
            {
                cleanedTitle += $" {{tmdb-{TvShowTmdb.Id}}}";
            }

            var directoryPath = Path.Combine(GetDirectoryManager()._BasePath, cleanedTitle);

            if (createDirectory)
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }

    public class Episode : IMedia
    {
        public Episode(int episodeNumber, int saisonNumber)
        {
            EpisodeNumber = episodeNumber;
            SaisonNumber = saisonNumber;
            DownloadedContents = new List<DownloadedContent>();
            DownloadableContents = new List<DownloadableContent>();
        }

        public int EpisodeNumber { get; set; }

        public int SaisonNumber { get; set; }

        public List<DownloadedContent> DownloadedContents { get; set; }

        public List<DownloadableContent> DownloadableContents { get; set; }

        public bool IsDownloaded => DownloadedContents.Any();

        public int NbFiles => DownloadedContents.Count;

        public string GetSeasonPath()
        {
            return $"Season {SaisonNumber.ToString("00")}";
        }
    }
}
