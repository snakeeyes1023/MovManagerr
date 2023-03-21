using MovManagerr.Core.Data;

namespace MovManagerr.Core.Services.Movies
{
    public interface IDownloadedMovieService
    {
        void DeleteUnfoundedDownload();
    }
}