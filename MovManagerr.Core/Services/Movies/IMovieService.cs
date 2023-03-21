using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Tasks.Backgrounds;
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
    }
}
