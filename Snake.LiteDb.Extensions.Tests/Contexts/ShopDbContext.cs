using Snake.LiteDb.Extensions.Mappers;
using Snake.LiteDb.Extensions.Mappers.Configurations.DbSetConfigurations;
using Snake.LiteDb.Extensions.Mappers.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.LiteDb.Extensions.Tests.Contexts
{
    public class ShopDbContext : LiteDbContext<ShopDbContext>
    {
        public ShopDbContext() : base("Filename=" + Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Movmanagerr"), "movmanagerr.db") + ";Connection=shared")
        {
            
        }

        public virtual LiteDbSet<CarEntity> CarEntities { get; private set; }


        
        protected override void Configure(IConfigureDbSet<ShopDbContext> configurer)
        {
            base.Configure(configurer);

            //configurer.Configure(x => x.CarEntities, x =>
            //{
            //    x.UseCustomConnectionString("Filename=" + Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Movmanagerr"), "movmanagerr.db") + ";Connection=shared");
            //});
        }
    }
}
