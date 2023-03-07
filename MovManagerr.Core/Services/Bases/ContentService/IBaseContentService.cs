using MovManagerr.Core.Data.Abstracts;
using Snake.LiteDb.Extensions.Mappers;

namespace MovManagerr.Core.Services.Bases.ContentService
{
    public interface IBaseContentService<T> where T : Content
    {
        DownloadedContent CreateDownloadedContent(T content, string loadInfoFrom, string path);
        IEnumerable<T> GetAll(int offset = 0, int limit = 0);
        IEnumerable<T> GetCandidates(SearchQuery searchQuery);
        int GetCount();
        LiteDbSet<T> GetCurrentCollection();
    }
}