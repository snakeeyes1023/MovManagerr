using LiteDB;
using MovManagerr.Core.Infrastructures.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.DataAccess.Repositories
{
    public class SettingRepository : BaseRepository<CustomSettings>, ISettingRepository
    {
        public SettingRepository(ILiteDatabase db) : base(db)
        {
        }

        public CustomSettings GetCurrentSettingOrCreate()
        {
            CustomSettings? settings = Collection.FindAll().FirstOrDefault();
            if (settings == null)
            {
                settings = new CustomSettings();
                return Create(settings);
            }
            return settings;
        }
    }

    public interface ISettingRepository : IBaseRepository<CustomSettings>
    {
        CustomSettings GetCurrentSettingOrCreate();
    }

}
