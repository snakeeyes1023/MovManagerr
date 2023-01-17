using MovManagerr.Core.Data;
using MovManagerr.Core.Downloaders.M3U;
using System.Linq.Expressions;

namespace MovManagerr.Core.Downloaders.Contents.Readers
{
    public class SerieReader : M3uContentReader<Episode>
    {

        protected override Expression<Func<MediaM3u, bool>> Filter()
        {
            return m => m.MuUrl.Contains("serie");
        }

        protected override Episode? BindDataInContent(MediaM3u mediaInfo)
        {
            var episode = new Episode();

            return episode;
        }
    }
}
