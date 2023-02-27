using MovManagerr.Core.Data.Abstracts;
using Snake.LiteDb.Extensions.Mappers;

namespace MovManagerr.Core.Services.Bases.ContentService
{
    public interface IBaseContentService<T> where T : Content
    {
        IEnumerable<T> GetAll(int offset = 0, int limit = 0, bool includeNotDownloaded = true);
        IEnumerable<T> GetCandidates(SearchQuery searchQuery, bool includeNotDownloaded = true);
        int GetCount(bool includeNotDownloaded = true);
        LiteDbSet<T> GetCurrentCollection();
    }
}