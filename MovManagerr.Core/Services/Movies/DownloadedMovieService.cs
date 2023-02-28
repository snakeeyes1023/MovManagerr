using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Services.Bases.ContentService;
using Snake.LiteDb.Extensions.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Services.Movies
{
    public class DownloadedMovieService : BaseContentService<Movie>, IDownloadedMovieService
    {
        public DownloadedMovieService(IContentDbContext contentDbContext) : base(contentDbContext)
        {
        }

        public override IEnumerable<Movie> GetAll(int offset, int limit)
        {
            return _currentCollection.UseQuery(x =>
            {
                x.Where(x => x.DownloadedContents.Count > 0);

                x.Limit(limit);
                x.Skip(offset);
                BaseOrderQuery(x);
            }).ToList();
        }

        public override int GetCount()
        {
            return _currentCollection.UseQuery(x => x.Where(x => x.DownloadedContents.Count > 0)).Count();
        }

        public override IEnumerable<Movie> GetCandidates(SearchQuery searchQuery)
        {
            var entities = _currentCollection.UseQuery(x =>
            {
                x.Where(x => x.Name.Contains(searchQuery.EnteredText));
                x.Where(x => x.DownloadedContents.Count > 0);

                x.Skip(searchQuery.Skip);
                x.Limit(searchQuery.Take);
            });

            return entities.ToList();
        }
    }
}
