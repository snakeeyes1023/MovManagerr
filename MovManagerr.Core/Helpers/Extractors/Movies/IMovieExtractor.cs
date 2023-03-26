namespace MovManagerr.Core.Helpers.Extractors.Movies
{
    public interface IMovieExtractor
    {
        FileMovieNameInfo ExtractFromFileName(string fileName);
    }
}