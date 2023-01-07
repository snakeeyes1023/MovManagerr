using M3USync.Data.Abstracts;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Data.Helpers
{
    public class DatabaseHelper
    {
        public static IMongoCollection<T> GetInstance<T>() where T : Entity
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");

            var db = client.GetDatabase("plexManage");

            //get attribute Table
            var table = (typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute)?.Name;

            if (string.IsNullOrEmpty(table))
            {
                throw new Exception("Table attribute is missing");
            }

            return db.GetCollection<T>(table);
        }
    }
}
