using LiteDB;

namespace MovManagerr.Core.Infrastructures.Dbs
{
    public class DbContext : IDisposable
    {
        private readonly string _connStr;
        private bool _disposed;


        private ILiteDatabase _db;
        private ILiteDatabase DB => _db ??= new LiteDatabase(_connStr);

        

        private IMovieRepository _moviesRepository;
        public IMovieRepository Movies
        {
            get { return _moviesRepository ??= new MovieRepository(DB); }
        }


        public DbContext(string connStr)
        {
            if (string.IsNullOrEmpty(connStr))
                throw new ArgumentNullException("missing connection string");

            _connStr = connStr;
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
    }
}
