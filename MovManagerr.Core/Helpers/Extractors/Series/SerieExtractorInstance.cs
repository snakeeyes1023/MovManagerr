using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Extractors.Series
{
    public class SerieExtractorInstance
    {
        public SerieExtractorInstance() { }

        public ISerieExtractor GetSerieInstratorInstance()
        {
            var extractors = new CombinedSerieExtractor();

            extractors.AddExtractor(new SerieExtractor(), 1);

            return extractors;
        }
    }
}
