using LiteDB;
using MovManagerr.Core.Data;

namespace MovManagerr.Core.Infrastructures.DataAccess.Repositories
{
    public class MovieRepository : BaseRepository<Movie>, IMovieRepository
    {
        public MovieRepository(ILiteDatabase db) : base(db)
        {
        }

        public Movie FindByTmdbId(int tmdbId)
        {
            return Collection.FindOne(x => x.TmdbId == tmdbId);
        }

        public IEnumerable<Movie> GetDownloadedMovies()
        {
            var movies = Collection.Find(x => x.IsDownloaded).ToList();
            return movies;
        }

        public Movie FindByFullPath(string fullPath)
        {
            return Collection.FindOne(x => x.DownloadedContents.Select(y => y.FullPath).Contains(fullPath));
        }
    }

    public interface IMovieRepository : IBaseRepository<Movie>
    {
        Movie FindByFullPath(string fullPath);
        Movie FindByTmdbId(int tmdbId);
        IEnumerable<Movie> GetDownloadedMovies();
    }
}
