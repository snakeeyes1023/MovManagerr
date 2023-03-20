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
    public class TvShow : Content
    {
        #region Tmdb

        public TMDbLib.Objects.Search.SearchTv? SearchTv { get; private set; }
        public DateTime? LastSearchAttempt { get; set; }
        public int TmdbId { get; set; }
        #endregion

        [BsonIgnore]
        public override List<DownloadedContent> DownloadedContents
        {
            get
            {
                return Episodes.SelectMany(x => x.DownloadedContents).ToList();
            }
            protected set
            {
                base.DownloadedContents = value;
            }
        }

        [BsonIgnore]
        public override List<DownloadableContent> DownloadableContents 
        {
            get
            {
                return Episodes.SelectMany(x => x.DownloadableContents).ToList();
            }
            protected set
            {
                base.DownloadableContents = value;
            }
        }

        public List<Episode> Episodes { get; protected set; }

        public override bool Equals(Content content)
        {
            return content is TvShow serie && serie == this;
        }

        public override DirectoryManager GetDirectoryManager()
        {
            return Preferences.Instance.Settings.GetContentPreference<TvShow>().GetDirectoryManager();
        }

        public override string GetPath(bool createDirectory = true)
        {
            string title = Name;
            string year = "0000";
            
            if (SearchTv != null)
            {
                title = SearchTv?.GetValidName() ?? Name;
                year = SearchTv != null && SearchTv.FirstAirDate.HasValue ? SearchTv.FirstAirDate.Value.Year.ToString() : "0000";
            }

            char[] invalidChars = Path.GetInvalidPathChars();
            var cleanedTitle = string.Join("_", title.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            // replace  by nothing in Regex.Replace(path, @"[ ]{2,}", " ");
            cleanedTitle = Regex.Replace(cleanedTitle, @"[ ]{2,}", " ").Replace(":", string.Empty);
            cleanedTitle = $"{cleanedTitle} ({year})";

            if (SearchTv != null)
            {
                cleanedTitle += $" {{tmdb-{SearchTv.Id}}}";
            }

            var directoryPath = Path.Combine(base.GetPath(createDirectory), cleanedTitle);

            if (createDirectory)
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }

    public class Episode
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

        public string GetSeasonPath()
        {
            return $"Season {SaisonNumber.ToString("00")}";
        }
    }
}
