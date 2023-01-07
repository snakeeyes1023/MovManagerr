using LiteDB;
using M3USync.Data.Abstracts;
using M3USync.Data.Helpers;
using M3USync.Downloaders.M3U;
using M3USync.Infrastructures.Configurations;
using System.Linq.Expressions;

namespace M3USync.Downloaders.Contents.Readers
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
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(Preferences.Instance._DbPath))
            {

                ILiteCollection<T> collection = DatabaseHelper.GetCollection<T>(db);
                IEnumerable<T> alreadyInDb = collection.FindAll();

                // Filter out items that are already in the database
                IEnumerable<T> newContents = Contents.Except(alreadyInDb, new ContentComparer<T>());

                // Insert new items into the database
                if (newContents.Any())
                {
                    collection.InsertBulk(newContents);

                    OnContentSynced?.Invoke(newContents);
                }
            }
            #endregion
        }

        public void Dispose()
        {
            Contents.Clear();
        }
    }
}
