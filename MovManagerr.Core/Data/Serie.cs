using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovManagerr.Core.Data
{
    public class SerieManager
    {
        private List<Serie> _series = new List<Serie>();

    }


    public class Serie
    {
        public string Name { get; set; }

        public List<Saison> Saison { get; set; }

        public void AppendEpisode(Episode episode)
        {

        }
    }

    public class Saison
    {
        public string Name { get; set; }

        public List<Episode> Episodes { get; set; }
    }

    [Table("episodes")]
    public class Episode 
    {
        public int EpisodeNumber { get; set; }

        public int SaisonNumber { get; set; }


    }
}
