using MovManagerr.Core.Helpers.Extractors.Shared;
using MovManagerr.Core.Infrastructures.Configurations;
using System.IO;

namespace MovManagerr.Core.Helpers.Extractors.Movies
{
    public class MovieExtractorInstance
    {
        public MovieExtractorInstance() { }

        public IMovieExtractor GetMovieInstratorInstance()
        {
            var extractors = new CombinedMovieExtractor();

            extractors.AddExtractor(new RadarrMovieExtractor(), 1);

            if (Preferences.Instance.Settings.UseOpenAI)
            {
                extractors.AddExtractor(new OpenAiMovieExtractor(), 1);
            }
            else
            {
                extractors.AddExtractor(new GeneralMovieExtractor(), 1);
            }

            return extractors;
        }
    }
}
