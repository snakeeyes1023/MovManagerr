using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Configurations;
using System.Linq.Expressions;

namespace MovManagerr.Core.Downloaders.Contents.Readers
{
    public abstract class M3uContentReader<T> : IReader where T : Content
    {
        #region Props
        protected List<T> Contents;
        protected Preferences Preferences;

        private readonly Func<MediaM3u, bool> s_isValid;
        #endregion

        public M3uContentReader()
        {
            s_isValid = Filter().Compile();
            Contents = new List<T>();
            Preferences = Preferences.Instance;

            using (var db = new LiteDatabase(Preferences._DbPath))
            {
                ILiteCollection<T> collection = DatabaseHelper.GetCollection<T>(db);

                Contents = collection.FindAll().ToList();
            }
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

                if (content is not null)
                {
                    T? existingContent = Contents.FirstOrDefault(c => c.Name == content?.Name);

                    if (existingContent == null)
                    {
                        Contents.Add(content);
                    }
                    else
                    {
                        existingContent.Merge(content);
                    }
                }
            }
        }


        public void SyncInDatabase()
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(Preferences.Instance._DbPath))
            {
                ILiteCollection<T> collection = DatabaseHelper.GetCollection<T>(db);

                foreach (var content in Contents)
                {
                    collection.Upsert(content);
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
