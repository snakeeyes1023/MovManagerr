using M3USync.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.UIs
{
    public class ContentCardContext : CardContext
    {
        public override void ApplyConfiguration(Collection<ICardConfiguration> configs)
        {
            var movieConfig = new CardConfiguration<Movie>();
            movieConfig.SetTitle(m => m.Name);

            configs.Add(movieConfig);
        }
    }
}
