using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.DataAccess.Repositories;
using MovManagerr.Core.Infrastructures.Dbs.Repositories;

namespace MovManagerr.Core.Infrastructures.Dbs
{
    public class DbContext : IDisposable
    {
        private readonly string _connStr;
        private bool _disposed;


        private ILiteDatabase _db;
        private ILiteDatabase DB => _db ??= new LiteDatabase(_connStr);

        
        private IMovieRepository _moviesRepository;
        public IMovieRepository Movies => _moviesRepository ??= new MovieRepository(DB);
        
        private IDownloadedContentRepository _downloadedContentRepository;
        public IDownloadedContentRepository DownloadedContents => _downloadedContentRepository ??= new DownloadedContentRepository(DB);
        
        private ISettingRepository _settingRepository;
        public ISettingRepository Settings => _settingRepository ??= new SettingRepository(DB);

        public DbContext()
        {
            if (string.IsNullOrEmpty(Preferences.Instance._DbPath))
                throw new ArgumentNullException("missing connection string");

            _connStr = Preferences.Instance._DbPath;

            Configure();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_db != null)
                        _db.Dispose();
                }
                _disposed = true;
            }
        }
        ~DbContext()
        {
            Dispose(false);
        }

        private void Configure()
        {
            BsonMapper.Global.Entity<Movie>()
                .Id(x => x.Id)
                .DbRef(x => x.Medias, "medias");
        }
    }
}
