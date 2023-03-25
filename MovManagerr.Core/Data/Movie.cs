using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Helpers.Extensions;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Tmdb;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Data
{
    public class Movie : IMedia
    { 
        public Movie(string name, string poster)
        {
            Name = name;
            Poster = poster;
            DownloadableContents = new List<DownloadableContent>();
            DownloadedContents = new List<DownloadedContent>();
        }

        public Movie()
        {
            DownloadableContents = new List<DownloadableContent>();
            DownloadedContents = new List<DownloadedContent>();
        }

        [BsonId]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Poster { get; set; }

        public string GetCorrectedPoster()
        {
            if (Poster != null && !Poster.StartsWith("http"))
            {
                return "https://image.tmdb.org/t/p/w200/" + Poster;

            }
            return Poster ?? string.Empty;
        }


        public List<DownloadedContent> DownloadedContents { get; protected set; }

        public List<DownloadableContent> DownloadableContents { get; protected set; }

        public bool IsDownloaded
        {
            get
            {
                return DownloadedContents.Count > 0;
            }
        }

        public int NbFiles
        {
            get
            {
                return this.DownloadedContents.Count;
            }
        }

        public decimal MaxBitrate
        {
            get
            {
                if (this.DownloadedContents.Any())
                {
                    return this.DownloadedContents.Max(x => x.OverallInfo.BitrateInMbs);
                }

                return 0;
            }
        }


        #region Tmdb
        public int TmdbId { get; set; }
        public TMDbLib.Objects.Movies.Movie? TmdbMovie { get; private set; }
        #endregion

        /// <summary>
        /// Gets the directory path.
        /// </summary>
        /// <returns></returns>
        public string GetPath(bool createDirectory = true)
        {
            var title = TmdbMovie?.GetValidName() ?? Name;

            var year = TmdbMovie != null && TmdbMovie.ReleaseDate.HasValue ? TmdbMovie.ReleaseDate.Value.Year.ToString() : "0000";

            char[] invalidChars = Path.GetInvalidPathChars();
            var path = string.Join("_", title.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            // replace  by nothing in Regex.Replace(path, @"[ ]{2,}", " ");
            path = Regex.Replace(path, @"[ ]{2,}", " ").Replace(":", string.Empty);
            path = $"{path} ({year})";

            var directoryPath = Path.Combine(GetDirectoryManager()._BasePath, path);

            if (createDirectory)
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        public bool IsInValidFolder()
        {
            foreach (var downloaded in DownloadedContents)
            {
                if (GetPath(false) != Path.GetDirectoryName(downloaded.FullPath))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Gets the directory manager.
        /// </summary>
        /// <returns></returns>
        public DirectoryManager GetDirectoryManager()
        {
            var preference = Preferences.Instance.Settings.GetContentPreference<Movie>();

            var directoryManager = preference.GetDirectoryManager();

            string folderForGenre = preference.GetFolderForGenre(TmdbMovie!.Genres.FirstOrDefault()?.Id ?? 0);

            if (string.IsNullOrWhiteSpace(folderForGenre))
            {
                return directoryManager;
            }
            else
            {
                return directoryManager.CreateSubInstance(folderForGenre);
            }
        }
        
        public static Movie CreateFromTmdbMovie(TMDbLib.Objects.Movies.Movie movie)
        {
            return new Movie(movie.GetValidName(), movie.PosterPath)
            {
                TmdbId = movie.Id,
                TmdbMovie = movie
            };
        }


        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual string GetFullPath(string fileName, bool createDirectory = true)
        {
            return Path.Combine(GetPath(createDirectory), fileName);
        }

        /// <summary>
        /// Représente l'emplacement ou son stocker les contenues du même type (exemple : les films dans le dossier "Films")
        /// </summary>
        /// <returns></returns>
        public void AddDownloadableContent(DownloadableContent downloable)
        {
            if (DownloadableContents.Any(x => downloable.Equals(x)))
            {
                return;
            }

            DownloadableContents.Add(downloable);
        }

        public void AddDownloadableContent(IEnumerable<DownloadableContent> downloads)
        {
            foreach (var downloable in downloads)
            {
                AddDownloadableContent(downloable);
            }
        }

        public void Download(IServiceProvider serviceProvider, DownloadableContent? downloadLink = null)
        {
            if (downloadLink == null)
            {
                downloadLink = DownloadableContents.FirstOrDefault();

                if (downloadLink == null)
                {
                    SimpleLogger.AddLog("Aucun fichier trouver", LogType.Error);
                    return;
                }
            }

            downloadLink.Download(serviceProvider, this);
        }

        public DownloadedContent CreateAndScan(string origin)
        {
            DownloadedContent download = new DownloadedContent(origin);
            download.LoadMediaInfo();

            DownloadedContents.Add(download);

            return download;
        }
    }
}
