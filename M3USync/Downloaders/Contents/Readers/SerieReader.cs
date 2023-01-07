using M3USync.Data;
using M3USync.Downloaders.M3U;
using System.Linq.Expressions;

namespace M3USync.Downloaders.Contents.Readers
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
