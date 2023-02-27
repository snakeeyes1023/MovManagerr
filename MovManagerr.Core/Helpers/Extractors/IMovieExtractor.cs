namespace MovManagerr.Core.Helpers.Extractors
{
    public interface IMovieExtractor
    {
        FileMovieNameInfo ExtractFromFileName(string fileName);
    }
}