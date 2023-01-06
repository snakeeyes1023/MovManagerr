using M3USync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands.Searchers
{
    public class EpisodeSearcherCommand : SearcherCommand<Episode>
    {
        private SerieManager SerieManager { get; set; } = new SerieManager();


        public EpisodeSearcherCommand() : base("Épisode")
        {
        }

    }
}
