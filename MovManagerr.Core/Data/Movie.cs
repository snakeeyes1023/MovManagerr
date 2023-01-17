using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Tmdb;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

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
        public override string GetDirectoryPath()
        {
            var title = string.IsNullOrEmpty(TmdbMovie?.Title) ? Name : TmdbMovie.Title;
            var year = TmdbMovie != null && TmdbMovie.ReleaseDate.HasValue ? TmdbMovie.ReleaseDate.Value.Year.ToString() : "0000";

            // Supprimer les caractères non autorisés dans le nom du répertoire, à l'exception des espaces
            title = Regex.Replace(title, @"[^\w\.@\s\(\)-]", "");


            return Path.Combine($"{title} ({year})");
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
            return Preferences.Instance.MovieManager;
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
    }
}
