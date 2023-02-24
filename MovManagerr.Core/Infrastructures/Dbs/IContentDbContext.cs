using MovManagerr.Core.Data;
using Snake.LiteDb.Extensions.Mappers;
using Snake.LiteDb.Extensions.Models;

namespace MovManagerr.Core.Infrastructures.Dbs
{
    public interface IContentDbContext
    {
        LiteDbSet<Episode> Episodes { get; }
        LiteDbSet<Movie> Movies { get; }

        LiteDbSet<C> GetCollection<C>() where C : Entity;
    }
}