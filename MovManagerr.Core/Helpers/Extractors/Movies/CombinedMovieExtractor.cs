namespace MovManagerr.Core.Helpers.Extractors.Movies
{
    public class CombinedMovieExtractor : IMovieExtractor
    {
        private readonly List<PriorityExtractorInstance> Extractors;

        public CombinedMovieExtractor()
        {
            Extractors = new List<PriorityExtractorInstance>();
        }

        public void AddExtractor(IMovieExtractor extractor, int priority)
        {
            Extractors.Add(new PriorityExtractorInstance(extractor, priority));
        }

        public FileMovieNameInfo ExtractFromFileName(string fileName)
        {
            var result = new FileMovieNameInfo();

            foreach (var extractor in Extractors.OrderBy(x => x.Priority))
            {
                result = extractor.Extractor.ExtractFromFileName(fileName);

                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        class PriorityExtractorInstance
        {
            internal PriorityExtractorInstance(IMovieExtractor extractor, int priority)
            {
                Extractor = extractor;
                Priority = priority;
            }

            internal IMovieExtractor Extractor { get; set; }
            internal int Priority { get; set; }
        }
    }
}
