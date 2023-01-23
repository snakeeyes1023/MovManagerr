using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.LiteDb.Extensions.Mappers.Configurations.DbSetConfigurations
{

    public interface IConfigureField
    {
        public void UseCustomConnectionString(string connectionString);
    }
}
