using MovManagerr.Core.Data.Abstracts;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Dbs;
using System.Linq.Expressions;

namespace MovManagerr.Core.Downloaders.Contents.Readers
{
    public abstract class M3uContentReader<T> where T : Content
    {
        #region Props
        protected Preferences Preferences;

        private readonly Func<MediaM3u, bool> s_isValid;

        private readonly BaseRepository<T> _repository;
        #endregion
        
        public M3uContentReader(BaseRepository<T> repository)
        {
            s_isValid = Filter().Compile();
            Preferences = Preferences.Instance;
            _repository = repository;
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
                    T? existingContent = _repository.FindOne(m => m.Name == content.Name);

                    if (existingContent == null)
                    {
                        _repository.Create(content);
                    }
                    else
                    {
                        existingContent.Merge(content);
                        _repository.Update(existingContent);
                    }
                }
            }
        }
        #endregion
    }
}
