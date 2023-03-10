using Snake.LiteDb.Extensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Snake.LiteDb.Extensions.Mappers
{
    public partial class LiteDbSet<T> : ILiteDbSet where T : Entity
    {
        public IEnumerable<T> ToList()
        {
            return UseQuery(x => x.ToList());
        }

        public T FirstOrDefault()
        {
            return UseQuery(x => x.FirstOrDefault());
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return UseQuery(x => x.Where(predicate).FirstOrDefault());
        }

        public bool Any()
        {
            return UseQuery(x => x.Count()) > 0;
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return UseQuery(x => x.Where(predicate).Count()) > 0;
        }

        public int Count()
        {
            return UseQuery(x => x.Count());
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return UseQuery(x => x.Where(predicate).Count());
        }

        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return UseQuery(x => x.Where(predicate).ToList());
        }
    }
}
