using MovManagerr.Core.Infrastructures.Configurations;

namespace MovManagerr.Core.Helpers.Extractors
{
    public class ExtractorInstance
    {
        public ExtractorInstance() { }

        public IMovieExtractor GetMovieInstratorInstance()
        {
            if (Preferences.Instance.Settings.UseOpenAI)
            {
                return new OpenAiMovieExtractor();
            }
            else
            {
                return new MovieExtractor();
            }
        }
    }
}
