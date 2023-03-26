using MovManagerr.Core.Infrastructures.Configurations;
using System.IO;

namespace MovManagerr.Core.Helpers.Extractors.Movies
{
    public class ExtractorInstance
    {
        public ExtractorInstance() { }

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
                extractors.AddExtractor(new MovieExtractor(), 1);
            }

            return extractors;
        }
    }
}
