using MovManagerr.Core.Helpers.Parsers;

namespace MovManagerr.Core.Helpers.Extractors.Movies
{
    public class RadarrMovieExtractor : IMovieExtractor
    {
        public FileMovieNameInfo ExtractFromFileName(string fileName)
        {
            var result = Parser.ParseMoviePath(fileName);

            if (result != null)
            {
                return new FileMovieNameInfo()
                {
                    MovieName = result.MovieTitle,
                    Year = result.Year,
                    Extensions = Path.GetExtension(fileName)
                };
            }
            return null;
        }
    }
}
