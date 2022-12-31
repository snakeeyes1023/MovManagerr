using M3USync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Commands.Searchers
{
    public class EpisodeSearcher : Searcher<Episode>
    {
        public EpisodeSearcher() : base("Épisode")
        {
        }
    }
}
