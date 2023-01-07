using LiteDB;
using M3USync.Data.Abstracts;
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
        public static ILiteCollection<T> GetCollection<T>(LiteDatabase liteDatabase) where T : Entity
        {
            //get attribute Table
            var table = (typeof(T)
                .GetCustomAttributes(typeof(TableAttribute), true)
                .FirstOrDefault() as TableAttribute)?.Name;

            if (string.IsNullOrEmpty(table))
            {
                throw new Exception("Table attribute is missing");
            }

            return liteDatabase.GetCollection<T>(table);
        }
    }
}
