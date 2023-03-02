using MovManagerr.Core.Data;
using MovManagerr.Core.Services.Bases.ContentService;

namespace MovManagerr.Core.Services.Movies
{
    public interface IDownloadedMovieService : IBaseContentService<Movie>
    {
        void DeleteUnfoundedDownload();
    }
}