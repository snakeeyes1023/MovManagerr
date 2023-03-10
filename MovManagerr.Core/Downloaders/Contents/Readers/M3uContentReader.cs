using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using Snake.LiteDb.Extensions.Mappers;
using System.Linq.Expressions;

namespace MovManagerr.Core.Downloaders.Contents.Readers
{
    public abstract class M3uContentReader<T> : IReader where T : Content
    {
        #region Props
        protected Preferences Preferences;

        private readonly Func<MediaM3u, bool> s_isValid;

        private readonly LiteDbSet<T> _contentDbContext;
        #endregion

        public M3uContentReader(IContentDbContext contentDbContext)
        {
            s_isValid = Filter().Compile();
            Preferences = Preferences.Instance;
            
            _contentDbContext = contentDbContext.GetCollection<T>();
        }

        #region Abstractions
        protected abstract Expression<Func<MediaM3u, bool>> Filter();

        protected abstract T? BindDataInContent(MediaM3u mediaInfo, string link);
        #endregion

        #region Implementations
        public void Read(MediaM3u mediaInfo, string link)
        {
            if (s_isValid(mediaInfo))
            {
                T? content = BindDataInContent(mediaInfo, link);

                if (content is not null)
                {
                    T? existingContent = _contentDbContext.FirstOrDefault(c => c.Name == content.Name);

                    if (existingContent == null)
                    {
                        _contentDbContext.Add(content);
                    }
                    else
                    {
                        existingContent.Merge(content);
                    }
                }
            }
        }


        public void SaveChanges()
        {
            _contentDbContext.SaveChanges();
        }

        public void Dispose()
        {
            _contentDbContext.ClearContext();
        }

        #endregion
    }
}
