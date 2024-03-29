﻿using LiteDB;
using MovManagerr.Core.Data;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.DataAccess.Repositories;

namespace MovManagerr.Core.Infrastructures.DataAccess
{
    public class DbContext : IDisposable
    {
        private readonly string _connStr;
        private bool _disposed;


        private ILiteDatabase _db;
        private ILiteDatabase DB => _db ??= new LiteDatabase(_connStr);


        private IMovieRepository _moviesRepository;
        public IMovieRepository Movies => _moviesRepository ??= new MovieRepository(DB);
        

        private ISettingRepository _settingRepository;
        public ISettingRepository Settings => _settingRepository ??= new SettingRepository(DB);

        public DbContext()
        {
            if (string.IsNullOrEmpty(Preferences.Instance._DbPath))
                throw new ArgumentNullException("missing connection string");

            _connStr = Preferences.Instance._DbPath;
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
