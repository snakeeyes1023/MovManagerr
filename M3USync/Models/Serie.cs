using M3USync.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Models
{
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
    public class Episode : Content
    {
        public int EpisodeNumber { get; set; }
        
        public int SaisonNumber { get; set; }

        public Episode(MediaM3u media) : base(media)
        {
        }

        public override string ExtractName(string muName)
        {
            var name = base.ExtractName(muName);

            var data = SerieHelper.ExtractSeasonAndEpisodeNumbers(name);

            EpisodeNumber = data.episodeNumber;
            SaisonNumber = data.seasonNumber;

            return data.show;
        }

        public override bool Equals(Content content)
        {
            var episode = content as Episode;

            return episode.Name == Name && episode.EpisodeNumber == EpisodeNumber && episode.SaisonNumber == SaisonNumber;
        }

        public override string GetDirectoryPath()
        {
            return $@"{Name}\Saison {SaisonNumber}\";
        }

        public override string GetFileName()
        {
            return $@"{Name} - Saison {SaisonNumber} - Episode {EpisodeNumber}.{GetExtension()}";
        }
    }
}
