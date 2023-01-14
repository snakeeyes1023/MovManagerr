using MovManagerr.Core.Data;

namespace MovManagerr.Cls.Commands.Searchers
{
    public class EpisodeSearcherCommand : SearcherCommand<Episode>
    {
        private SerieManager SerieManager { get; set; } = new SerieManager();


        public EpisodeSearcherCommand() : base("Épisode")
        {
        }

    }
}
