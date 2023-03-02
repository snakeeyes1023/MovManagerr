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
            return _currentCollection.UseQuery(x =>
            {
                x.Where(x => x.DownloadedContents.Count > 0);

                if (limit > 0)
                {
                    x.Limit(limit);
                }
                if (offset > 0)
                {
                    x.Skip(offset);
                }

                BaseOrderQuery(x);
            }).ToList().Where(x => x.IsDownloaded);
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

                        movie.SetDirty();
                        _currentCollection.TrackEntity(movie);
                        _currentCollection.SaveChanges();
                    }
                }
            }
        }
    }
}
