using MovManagerr.Core.Data;
using MovManagerr.Core.Infrastructures.Configurations;
using Snake.LiteDb.Extensions.Mappers;
using Snake.LiteDb.Extensions.Mappers.DbContexts;

namespace MovManagerr.Core.Infrastructures.Dbs
{
    public class ContentDbContext : LiteDbContext<ContentDbContext>, IContentDbContext
    {
        public virtual LiteDbSet<Movie> Movies { get; private set; }
        public virtual LiteDbSet<CustomSettings> Settings { get; private set; }

        public ContentDbContext() : base(Preferences.Instance._DbPath)
        {
        }

        public ContentDbContext(string dbPath) : base(dbPath)
        {
        }
    }
}
