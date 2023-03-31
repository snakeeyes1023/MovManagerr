using MovManagerr.Core.Helpers.Extractors.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Extractors.Series
{

    public interface ISerieExtractor : IExtractor
    {
        public IExtractionResult ScanFolder(string path);
    }

    public class CombinedSerieExtractor : CombinedExtractor<ISerieExtractor>, ISerieExtractor
    {
        public IExtractionResult ScanFolder(string path)
        {
            return ExtractWithCustomMethod(x => x.ScanFolder(path));
        }
    }


    public class SerieAnalyseResult : IExtractionResult    
    {
        public SerieAnalyseResult()
        {
            Episodes = new List<EpisodeAnalyseResult>();
        }
        
        public int TotalExtractedData { get; set; }
        public string SerieName { get; set; }
        public string SeriePath { get; set; }
        public List<EpisodeAnalyseResult> Episodes { get; set; }
    }

    public class EpisodeAnalyseResult : IExtractionResult
    {
        public string EpisodeName { get; set; }
        public string EpisodePath { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }

        public int TotalExtractedData { get; set; }
    }
}
