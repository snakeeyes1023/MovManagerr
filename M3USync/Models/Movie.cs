using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using M3USync.Config;
using M3USync.Models.Enums;
using M3USync.Models.Intefaces;
using Microsoft.VisualBasic;
using MongoDB.Bson.Serialization.Attributes;
using MovManagerr.Tmdb;

namespace M3USync.Models
{
    [Table("movies")]
    public class Movie : Content
    {
        private TMDbLib.Objects.Search.SearchMovie? _movie;

        [BsonIgnore]
        public TMDbLib.Objects.Search.SearchMovie? TmdbMovie
        {
            get
            {
                if (_movie == null)
                {
                    _movie = SearchMovie();
                }

                return _movie;
            }
            set { _movie = value; }
        }

        public Movie(MediaM3u media) : base(media)
        {
            Type = ContentType.Movie;
        }

        public Movie()
        {
            Type = ContentType.Movie;
        }

        public override bool Equals(Content content)
        {
            return content is Movie movie && movie.Url == Url;
        }

        public override string GetDirectoryPath()
        {
            var title = string.IsNullOrEmpty(TmdbMovie?.Title) ? Name : TmdbMovie.Title;
            var year = TmdbMovie != null && TmdbMovie.ReleaseDate.HasValue ? TmdbMovie.ReleaseDate.Value.Year.ToString() : "0000";

            return Path.Combine(Preferences.Instance.MovieFolder, $"{title} ({year})");
        }

        public override string GetFileName()
        {
            // Récupère le titre et l'extension de l'url
            string title = Path.GetFileNameWithoutExtension(Url);
            string extension = Path.GetExtension(Url);

            // Si aucun titre n'a été trouvé, utilise le titre de TmdbMovie
            if (string.IsNullOrEmpty(title))
            {
                title = TmdbMovie?.Title;
            }

            // Si aucune extension n'a été trouvée, utilise "mp4" par défaut
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".mp4";
            }

            // Retourne le titre et l'extension concaténés
            return $"{title}{extension}";
        }

        public TMDbLib.Objects.Search.SearchMovie? SearchMovie()
        {           
            if (Name != null)
            {
                var result = Preferences.GetTmdbInstance().GetMovieByNameAsync(Name).Result;
                TMDBID = (result?.Id ?? 0).ToString();

                return result;
            }

            return null;
        }
    }
}
