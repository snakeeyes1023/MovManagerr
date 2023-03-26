//using MovManagerr.Core.Data;
//using MovManagerr.Core.Downloaders.M3U;
//using MovManagerr.Core.Infrastructures.DataAccess;
//using System.Linq.Expressions;

//namespace MovManagerr.Core.Downloaders.Contents.Readers
//{
//    public class SerieReader : M3uContentReader<TvShow>
//    {
//        public SerieReader(DbContext contentDbContext) : base(contentDbContext)
//        {
//        }
        
//    //    protected override Expression<Func<MediaM3u, bool>> Filter()
//    //    {
//    //        return m => m.MuUrl.Contains("serie");
//    //    }

//        protected override TvShow? BindDataInContent(MediaM3u mediaInfo, string source)
//        {
//            var episode = new TvShow();

//            return episode;
//        }
//    }
//}
