using LiteDB;

namespace MovManagerr.Core.Data.Abstracts
{
    public abstract class Entity
    {
        public ObjectId _id { get; set; }

        [BsonIgnore]
        public bool IsDirty { get; private set; }

        public abstract void Merge(Entity content);

        public void SetDirty(bool isDirty = true)
        {
            IsDirty = isDirty;
        }
    }
}
