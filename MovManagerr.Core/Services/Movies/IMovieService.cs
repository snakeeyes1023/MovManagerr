using MovManagerr.Core.Data;
using TMDbLib.Objects.Search;

namespace MovManagerr.Core.Services.Movies
{
    public interface IMovieService
    {
        void ScanFolder(string path);
        void ReorganiseFolder();
        void Schedule_ReorganiseFolder();
        IEnumerable<SearchMovie?> GetMatchForFileName(string filename);
        Movie GetMovieFromSearchMovie(SearchMovie info);
        void DeleteUnfoundedDownload();
        Movie GetMovieFromTDMBMovie(TMDbLib.Objects.Movies.Movie info);
    }
}
