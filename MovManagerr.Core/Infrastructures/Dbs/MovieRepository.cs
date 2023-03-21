using LiteDB;
using MovManagerr.Core.Data;

namespace MovManagerr.Core.Infrastructures.Dbs
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
    }

    public interface IMovieRepository : IBaseRepository<Movie>
    {
        Movie FindByTmdbId(int tmdbId);
    }
}
