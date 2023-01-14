using MovManagerr.Core.Data.Abstracts;

namespace MovManagerr.Core.Services.Bases.ContentService
{
    public interface IBaseContentService<T> where T : Content
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetCandidates(SearchQuery searchQuery);
    }
}