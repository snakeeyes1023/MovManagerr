﻿using LiteDB;

namespace Snake.LiteDb.Extensions.Models
{
    public abstract class Entity
    {
        public ObjectId _id { get; set; }

        [BsonIgnore]
        public bool IsDirty { get; private set; }

        public void SetDirty(bool isDirty = true)
        {
            IsDirty = isDirty;
        }
    }
}
