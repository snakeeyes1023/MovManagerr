using MovManagerr.Core.Infrastructures.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
