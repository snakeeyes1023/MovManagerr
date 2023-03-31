using MovManagerr.Core.Helpers.Extractors.Shared;

namespace MovManagerr.Core.Helpers.Extractors.Movies
{
    public class FileMovieNameInfo : IExtractionResult
    {
        public string MovieName { get; set; }
        public int Year { get; set; }
        public string Extensions { get; set; }
        public int TotalExtractedData { get; set; } = 0;
    }
}
