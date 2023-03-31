using Microsoft.Extensions.Logging;
using MovManagerr.Core.Infrastructures.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Parsers.Quality
{
    public static class QualityFinder
    {
        public static Quality FindBySourceAndResolution(Source source, int resolution, Modifier modifer)
        {
            // Check for a perfect 3-way match
            var matchingQuality = Quality.All.SingleOrDefault(q => q.Source == source && q.Resolution == resolution && q.Modifier == modifer);

            if (matchingQuality != null)
            {
                return matchingQuality;
            }

            // Check for Source and Modifier Match for Qualities with Unknown Resolution
            var matchingQualitiesUnknownResolution = Quality.All.Where(q => q.Source == source && (q.Resolution == 0) && q.Modifier == modifer && q != Quality.Unknown);

            if (matchingQualitiesUnknownResolution.Any())
            {
                if (matchingQualitiesUnknownResolution.Count() == 1)
                {
                    return matchingQualitiesUnknownResolution.First();
                }

                foreach (var quality in matchingQualitiesUnknownResolution)
                {
                    if (quality.Source >= source)
                    {
                        SimpleLogger.Debug("Unable to find exact quality for {0},  {1}, and {2}. Using {3} as fallback"/*, source, resolution, modifer, quality*/);
                        return quality;
                    }
                }
            }

            // Check for Modifier match
            var matchingModifier = Quality.All.Where(q => q.Modifier == modifer);

            var matchingResolution = matchingModifier.Where(q => q.Resolution == resolution)
                                            .OrderBy(q => q.Source)
                                            .ToList();

            var nearestQuality = Quality.Unknown;

            foreach (var quality in matchingResolution)
            {
                if (quality.Source >= source)
                {
                    nearestQuality = quality;
                    break;
                }
            }

            SimpleLogger.Debug("Unable to find exact quality for {0},  {1}, and {2}. Using {3} as fallback"/*, source, resolution, modifer, nearestQuality*/);

            return nearestQuality;
        }
    }
}
