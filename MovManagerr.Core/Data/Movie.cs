using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Tmdb;
using Snake.LiteDb.Extensions.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Data
{
    [Table("movies")]
    public class Movie : Content
    {
        public Movie(string name, string poster) : base(name, poster)
        {
        }

        public Movie()
        {
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
        public override string GetPath(bool createDirectory = true)
        {
            var title = string.IsNullOrEmpty(TmdbMovie?.OriginalTitle) ? Name : TmdbMovie.OriginalTitle;
            var year = TmdbMovie != null && TmdbMovie.ReleaseDate.HasValue ? TmdbMovie.ReleaseDate.Value.Year.ToString() : "0000";

            char[] invalidChars = Path.GetInvalidPathChars();
            var path = string.Join("_", title.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            // replace  by nothing in Regex.Replace(path, @"[ ]{2,}", " ");

            path = Regex.Replace(path, @"[ ]{2,}", " ").Replace(":", string.Empty);
            path = $"{path} ({year})";

            var directoryPath = Path.Combine(base.GetPath(createDirectory), path);

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
        public override DirectoryManager GetDirectoryManager()
        {
            var preference = Preferences.Instance.Settings.GetContentPreference<Movie>();

            return preference
                      .GetDirectoryManager()
                      .CreateSubInstance(preference.GetFolderForGenre(TmdbMovie!.GenreIds.FirstOrDefault()));
        }

        /// <summary>
        /// Equalses the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public override bool Equals(Content content)
        {
            return content is Movie movie && movie == this;
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

        public override void Merge(Entity entity)
        {
            if (entity is Movie movie)
            {
                base.Merge(entity);

                if (movie.TmdbMovie != null && movie.TmdbId != 0)
                {
                    TmdbMovie = movie.TmdbMovie;
                    TmdbId = movie.TmdbId;
                }
            }
            else
            {
                throw new ArgumentException("The entity is not a Movie");
            }
        }

        public static Movie CreateFromSearchMovie(SearchMovie searchMovie)
        {
            return new Movie(searchMovie.OriginalTitle, searchMovie.PosterPath)
            {
                TmdbId = searchMovie.Id,
                TmdbMovie = searchMovie
            };
        }
    }
}
