using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using LiteDB;
using M3USync.Data.Abstracts;
using M3USync.Data.Enums;
using M3USync.Downloaders.M3U;
using M3USync.Infrastructures.Configurations;
using M3USync.Models.Intefaces;
using Microsoft.VisualBasic;
using MovManagerr.Tmdb;

namespace M3USync.Data
{
    [Table("movies")]
    public class Movie : Content
    {
        private TMDbLib.Objects.Search.SearchMovie? _movie;

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

            // Supprimer les caractères non autorisés dans le nom du répertoire, à l'exception des espaces
            title = Regex.Replace(title, @"[^\w\.@\s\(\)-]", "");


            return Path.Combine($"{title} ({year})");
        }

        public override string GetFileName()
        {
            // Récupère le titre et l'extension de l'url
            string title = Path.GetFileNameWithoutExtension(Url);
            string extension = Path.GetExtension(Url);

            // Si aucun titre n'a été trouvé, utilise le titre de TmdbMovie
            if (string.IsNullOrEmpty(title))
            {
                title = TmdbMovie?.Title ?? Name;
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
            if (Name != null && Preferences.GetTmdbInstance() is TmdbClientService client)
            {
                var movie = client.GetMovieByName(Name);

                if (movie != null)
                {
                    TMDBID = movie.Id.ToString();
                    
                    return movie;
                }
            }

            return null;
        }

        public override DirectoryManager GetDirectoryManager()
        {
            return Preferences.Instance.MovieManager;
        }
    }
}
