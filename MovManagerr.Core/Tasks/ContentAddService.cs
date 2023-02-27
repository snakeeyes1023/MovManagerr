using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Tmdb;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Tasks
{
    public class ContentAddService
    {
        private readonly IContentDbContext _contentDbContext;

        public ContentAddService(IContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public string ImportMovie(string fullPath, SearchMovie info)
        {
            var movie = _contentDbContext.Movies.UseQuery(x =>
            {
                x.Where(movie => movie.TmdbId == info.Id);
            }).ToList().FirstOrDefault();

            if (movie == null)
            {
                movie = Movie.CreateFromSearchMovie(info);

                _contentDbContext.Movies.Add(movie);
                _contentDbContext.Movies.SaveChanges();
            }

            _contentDbContext.Movies.TrackEntity(movie);

            var path = movie.GetFullPath(Path.GetFileName(fullPath));

            movie.DownloadedContents.Add(new DownloadedContent(path));

            movie.SetDirty();

            _contentDbContext.Movies.SaveChanges();

            SimpleLogger.AddLog($"Nouveau film ajouté à Plex : {info.OriginalTitle}!", LogType.Info);

            return path;
        }

        public IEnumerable<SearchMovie?> GetMatchForFileName(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename) && Preferences.GetTmdbInstance() is TmdbClientService client)
            {
                return client.GetRelatedMovies(filename) ?? new List<SearchMovie>();
            }

            return new List<SearchMovie>();
        }
    }
}
