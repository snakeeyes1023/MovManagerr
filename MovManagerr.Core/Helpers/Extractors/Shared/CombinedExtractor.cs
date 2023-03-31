using MovManagerr.Core.Helpers.Extractors.Movies;

namespace MovManagerr.Core.Helpers.Extractors.Shared
{
    public class CombinedExtractor<Extractor> : IExtractor
        where Extractor : IExtractor
    {
        private readonly List<PriorityExtractorInstance> Extractors;

        public CombinedExtractor()
        {
            Extractors = new List<PriorityExtractorInstance>();
        }

        public void AddExtractor(Extractor extractor, int priority)
        {
            Extractors.Add(new PriorityExtractorInstance(extractor, priority));
        }

        public virtual IExtractionResult ExtractFromFileName(string fileName)
        {
            return ExtractWithCustomMethod(x => x.ExtractFromFileName(fileName));
        }

        protected IExtractionResult ExtractWithCustomMethod(Func<Extractor, IExtractionResult> extractionAction)
        {
            var results = new List<IExtractionResult>();

            foreach (var extractor in Extractors.OrderByDescending(x => x.Priority))
            {
                var result = extractionAction.Invoke(extractor.Extractor);

                if (result != null)
                {
                    results.Add(result);
                }
            }

            if (results.Any() && results.Count > 0)
            {
                return results
                    .OrderByDescending(x => x.TotalExtractedData)
                    .FirstOrDefault()!;
            }
            else
            {
                throw new ArgumentException("No extractor was able to extract data from the file name.");
            }
        }

        class PriorityExtractorInstance
        {
            internal PriorityExtractorInstance(Extractor extractor, int priority)
            {
                Extractor = extractor;
                Priority = priority;
            }

            internal Extractor Extractor { get; set; }
            internal int Priority { get; set; }
        }
    }
}
