using M3USync.Config;
using M3USync.Data;
using M3USync.Http;
using M3USync.Http.Models;
using M3USync.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.Movies;

namespace M3USync.Readers
{
    public abstract class M3uContentReader<T> : IReader where T : Content
    {
        #region Props
        protected List<T> Contents;
        protected Preferences Preferences;

        private readonly Func<MediaM3u, bool> s_isValid;
        #endregion

        #region Events
        public event Action OnContentProceeded;
        public event Action<IEnumerable<T>> OnContentSynced;
        #endregion

        public M3uContentReader()
        {
            s_isValid = Filter().Compile();
            Contents = new List<T>();
            Preferences = Preferences.Instance;
        }

        #region Abstractions
        protected abstract Expression<Func<MediaM3u, bool>> Filter();

        protected abstract T? BindDataInContent(MediaM3u mediaInfo);
        #endregion

        #region Implementations
        public void Read(MediaM3u mediaInfo)
        {
            if (s_isValid(mediaInfo))
            {
                T? content = BindDataInContent(mediaInfo);

                if (content != null
                    && !Contents.Any(x => x.Equals(content))
                    && Preferences.Langs.Any(l => content.Tags.Contains(l)))
                {
                    Contents.Add(content);
                    OnContentProceeded?.Invoke();
                }
            }
        }

        public void SyncInDatabase()
        {
            try
            {
                IMongoCollection<T> collection = GetCollections();
                IEnumerable<T> alreadyInDb = collection.Find(x => true).ToList();

                if (Contents.Any())
                {
                    // Filter out items that are already in the database
                    IEnumerable<T> newContents = Contents.Where(x => !alreadyInDb.Any(y => y.Equals(x)));

                    // Insert new items into the database
                    if (newContents.Any())
                    {
                        collection.InsertMany(newContents);

                        OnContentSynced?.Invoke(newContents);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected IMongoCollection<T> GetCollections()
        {
            return DatabaseHelper.GetInstance<T>();
        }

        public void Dispose()
        {
            Contents.Clear();
        }

        #endregion
    }
}

