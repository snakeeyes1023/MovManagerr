using LiteDB;

namespace MovManagerr.Core.Data.Abstracts
{
    public abstract class Entity
    {
        public ObjectId _id { get; set; }
        
        public abstract void Merge(Entity content);

    }
}
