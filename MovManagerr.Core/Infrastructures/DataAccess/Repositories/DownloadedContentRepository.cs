using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Dbs.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.DataAccess.Repositories
{

    public class DownloadedContentRepository : BaseRepository<DownloadedContent>, IDownloadedContentRepository
    {
        public DownloadedContentRepository(ILiteDatabase db) : base(db)
        {
        }

        public DownloadedContent CreateAndScan(Movie movie, string origin)
        {
            DownloadedContent download = new DownloadedContent(origin);
            download.MovieId = movie.Id;
            download.LoadMediaInfo();
            return Create(download);
        }

    }

    public interface IDownloadedContentRepository : IBaseRepository<DownloadedContent>
    {
        DownloadedContent CreateAndScan(Movie movie, string origin);
    }
}
