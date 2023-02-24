using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.LiteDb.Extensions.Mappers
{
    public interface ILiteDbSet
    {
        string ConnectionStrings { get; internal set; }
    }
}
