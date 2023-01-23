using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Services.Bases.ContentService;
using MovManagerr.Core.Tasks.Backgrounds;

namespace MovManagerr.Core.Services.Movies
{
    public interface IMovieService : IBaseContentService<Movie>
    {
        IEnumerable<Movie> GetRecent(int limit);
        EventedBackgroundService GetSearchAllMovieOnTmdbService();
        EventedBackgroundService GetSyncM3UFilesInDbService();
        Movie? GetMovieById(ObjectId _id);
    }
}
