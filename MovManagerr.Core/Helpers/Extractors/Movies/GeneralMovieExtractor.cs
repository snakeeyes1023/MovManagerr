using MovManagerr.Core.Helpers.Extractors.Shared;

namespace MovManagerr.Core.Helpers.Extractors.Movies
{
    public class GeneralMovieExtractor : IMovieExtractor
    {
        public IExtractionResult ExtractFromFileName(string fileName)
        {
            return new FileMovieNameInfo()
            {
                MovieName = Path.GetFileNameWithoutExtension(fileName),
                Extensions = Path.GetExtension(fileName)
            };
        }

        public IExtractionResult ExtractFromFolder(string path)
        {
            throw new NotImplementedException();
        }
    }

    public interface IMovieExtractor : IExtractor
    {
        
    }

    public class CombinedMovieExtractor : CombinedExtractor<IMovieExtractor>, IMovieExtractor
    {
        
    }
}
