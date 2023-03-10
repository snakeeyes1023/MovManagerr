using Hangfire;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Infrastructures.Loggers;
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
            return _currentCollection.UseQuery(query => query.Where(x => x.DownloadedContents.Count > 0).GetRange(offset, limit).ToList());
        }

        public override int GetCount()
        {
            return _currentCollection.Where(x => x.DownloadedContents.Count > 0).Count();
        }

        public override IEnumerable<Movie> GetCandidates(SearchQuery searchQuery)
        {
            return _currentCollection
                .UseQuery(query => query
                    .Where(x => x.Name.Contains(searchQuery.EnteredText) && x.DownloadedContents.Count > 0)
                    .Skip(searchQuery.Skip)
                    .Limit(searchQuery.Take)
                    .ToList()
                );
        }

        public void Schedule_DeleteUnfoundedDownload()
        {
            BackgroundJob.Enqueue(() => DeleteUnfoundedDownload());
        }

        public void DeleteUnfoundedDownload()
        {
            var movies = GetAll(0, 0);

            foreach (var movie in movies)
            {
                if (movie != null)
                {
                    List<DownloadedContent> toDelete = new List<DownloadedContent>();
                    foreach (var download in movie.DownloadedContents)
                    {
                        if (download != null && !File.Exists(download.FullPath))
                        {
                            SimpleLogger.AddLog($"Le fichier {download.FullPath} est introuvable! Suppression imminente.");
                            toDelete.Add(download);
                        }
                    }

                    if (toDelete.Any())
                    {
                        foreach (var download in toDelete)
                        {
                            movie.DownloadedContents.Remove(download);
                        }

                        _currentCollection.UpdateEntity(movie);
                        _currentCollection.SaveChanges();
                    }
                }
            }
        }
    }
    public static class Tst
    {
        public static LiteDB.ILiteQueryableResult<Movie> GetRange(this LiteDB.ILiteQueryable<Movie> query, int offset, int limit)
        {
            return query.Skip(offset).Limit(limit);
        }
    }
}
