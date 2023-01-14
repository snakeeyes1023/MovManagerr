using LiteDB;
using MovManagerr.Core.Data.Abstracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovManagerr.Core.Data.Helpers
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
