using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Downloaders.M3U;
using System.Linq.Expressions;

namespace MovManagerr.Core.Downloaders.Contents.Readers
{
    public class MovieReader : M3uContentReader<Movie>
    {
        public MovieReader(IContentDbContext contentDbContext) : base(contentDbContext)
        {
        }

        protected override Movie? BindDataInContent(MediaM3u mediaInfo, string link)
        {
            //movie data
            string movieName = (mediaInfo.MuName.Split("|").Last() ?? "").Trim();
            string poster = mediaInfo.MuLogo;

            var movie = new Movie(movieName, poster);

            //link data
            List<string> linkTags = mediaInfo.MuName.Split("|").ToList().Where(x => string.IsNullOrWhiteSpace(x) == false).Select(x => x.Trim()).ToList();
            linkTags.RemoveAt(linkTags.Count - 1);

            movie.AddDownloadableContent(new M3UContentLink() { Link = mediaInfo.MuUrl, Tags = linkTags, Source = link });

            movie.AddCustomData("parsedFrom", mediaInfo.MuFullContent);

            return movie;
        }

        protected override Expression<Func<MediaM3u, bool>> Filter()
        {
            return m => m.MuUrl.Contains("movie") && m.MuName.Contains("|FR|", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}