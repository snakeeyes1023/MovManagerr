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
    [Table("movies")]
    public class Movie
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

        public Dictionary<string, object> CustomData { get; protected set; }

        public bool IsDownloaded
        {
            get
            {
                return DownloadedContents.Count > 0;
            }
        }

        public decimal MaxBitrate
        {
            get
            {
                return this.GetMaxBitrate();
            }
        }

        public decimal FileSize
        {
            get
            {
                return this.DownloadedContents.Sum(x => x.FileSizeAsGb);
            }
        }

        public int NbFiles
        {
            get
            {
                return this.DownloadedContents.Count;
            }
        }


        #region Tmdb
        public int TmdbId { get; set; }
        public TMDbLib.Objects.Search.SearchMovie? TmdbMovie { get; private set; }
        public DateTime? LastSearchAttempt { get; set; }
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
        /// Searches the movie on TMDB. (base on the name)
        /// </summary>
        /// <returns></returns>
        public void SearchMovieOnTmdb()
        {
            LastSearchAttempt = DateTime.Now;

            if (Name != null && Preferences.GetTmdbInstance() is TmdbClientService client)
            {
                TmdbMovie = client.GetMovieByName(Name);

                if (TmdbMovie != null)
                {
                    TmdbId = TmdbMovie.Id;
                }
            }
        }

        /// <summary>
        /// Gets the directory manager.
        /// </summary>
        /// <returns></returns>
        public DirectoryManager GetDirectoryManager()
        {
            var preference = Preferences.Instance.Settings.GetContentPreference<Movie>();

            var directoryManager = preference.GetDirectoryManager();

            string folderForGenre = preference.GetFolderForGenre(TmdbMovie!.GenreIds.FirstOrDefault());

            if (string.IsNullOrWhiteSpace(folderForGenre))
            {
                return directoryManager;
            }
            else
            {
                return directoryManager.CreateSubInstance(folderForGenre);
            }
        }

        public bool IsSearchedOnTmdb()
        {
            return LastSearchAttempt.HasValue || TmdbMovie != null;
        }

        public static Expression<Func<Movie, bool>> GetIsSearchOnTmdbExpressionEnable(bool isSearched)
        {
            if (isSearched)
            {
                return x => x.LastSearchAttempt.HasValue || x.TmdbMovie != null;
            }
            return x => !x.LastSearchAttempt.HasValue || x.TmdbMovie != null;
        }


        public bool IsSearchedOnTmdbFailed()
        {
            return LastSearchAttempt.HasValue && TmdbMovie == null;
        }

        public static Movie CreateFromSearchMovie(SearchMovie searchMovie)
        {
            return new Movie(searchMovie.GetValidName(), searchMovie.PosterPath)
            {
                TmdbId = searchMovie.Id,
                TmdbMovie = searchMovie
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

        public void AddCustomData(string key, object data)
        {
            if (CustomData == null)
            {
                CustomData = new Dictionary<string, object>();
            }

            CustomData.Add(key, data);
        }

        public T? GetCustomData<T>(string key)
        {
            if (CustomData != null && CustomData.GetValueOrDefault(key) is T data)
            {
                return data;
            }

            return default;
        }

        public string[] GetCombinedTags()
        {
            return DownloadableContents
                     .OfType<M3UContentLink>()
                     .SelectMany(x => x.Tags)
                     .Distinct()
                     .ToArray();
        }
        
        public virtual void Merge(Movie entity)
        {
            if (entity is Movie content)
            {
                AddDownloadableContent(content.DownloadableContents);
                Poster = content.Poster;

                if (content.TmdbMovie != null && content.TmdbId != 0)
                {
                    TmdbMovie = content.TmdbMovie;
                    TmdbId = content.TmdbId;
                }

                if (CustomData == null)
                {
                    CustomData = content.CustomData;
                }
                else
                {
                    foreach (var item in content.CustomData)
                    {
                        if (!CustomData.ContainsKey(item.Key))
                        {
                            CustomData.Add(item.Key, item.Value);
                        }
                        else
                        {
                            CustomData[item.Key] = item.Value;
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot merge entity with different type");
            }
        }
    }
}
