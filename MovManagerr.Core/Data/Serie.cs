using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Configurations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovManagerr.Core.Data
{
    public class SerieManager
    {
        private List<Serie> _series = new List<Serie>();

        public void AddEpisode(Episode episode)
        {
            var serie = _series.FirstOrDefault(x => x.Name == episode.Name);
            if (serie == null)
            {
                serie = new Serie
                {
                    Name = episode.Name,
                    Saison = new List<Saison>()
                };
                _series.Add(serie);
            }

            var saison = serie.Saison.FirstOrDefault(x => x.Name == $"Saison {episode.SaisonNumber}");
            if (saison == null)
            {
                saison = new Saison
                {
                    Name = $"Saison {episode.SaisonNumber}",
                    Episodes = new List<Episode>()
                };
                serie.Saison.Add(saison);
            }

            saison.Episodes.Add(episode);
        }
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
    public class Episode : Content
    {
        public int EpisodeNumber { get; set; }

        public int SaisonNumber { get; set; }

        //public override string ExtractName(string muName)
        //{
        //    var name = base.ExtractName(muName);

        //    var data = SerieHelper.ExtractSeasonAndEpisodeNumbers(name);

        //    EpisodeNumber = data.episodeNumber;
        //    SaisonNumber = data.seasonNumber;

        //    return data.show;
        //}

        public override bool Equals(Content content)
        {
            if (content is not Episode episode) return false;

            return episode.Name == Name && episode.EpisodeNumber == EpisodeNumber && episode.SaisonNumber == SaisonNumber;
        }

        public override string GetPath(bool createdDirectory = true)
        {
            return $@"{Name}\Saison {SaisonNumber}\";
        }

        //public override string GetFileName()
        //{
        //    return $@"{Name} - Saison {SaisonNumber} - Episode {EpisodeNumber}.{GetExtension()}";
        //}

        public override DirectoryManager GetDirectoryManager()
        {
            // return Preferences.Instance.Settings.GetContentPreference<>().GetDirectoryManager();
            throw new NotImplementedException();
        }
    }
}
