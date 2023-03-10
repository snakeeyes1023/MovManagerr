using ChangeTracking;
using LiteDB;

namespace Snake.LiteDb.Extensions.Models
{
    public abstract class Entity
    {
        [DoNoTrack]
        public virtual ObjectId _id { get; set; }
    }
}
