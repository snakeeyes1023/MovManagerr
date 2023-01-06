using M3USync.Http.Models;
using M3USync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Readers
{
    public class SerieReader : M3uContentReader<Episode>
    {

        protected override Expression<Func<MediaM3u, bool>> Filter()
        {
            return m => m.MuUrl.Contains("serie");
        }

        protected override Episode? BindDataInContent(MediaM3u mediaInfo)
        {
            var episode = new Episode(mediaInfo);

            return episode;
        }
    }
}
