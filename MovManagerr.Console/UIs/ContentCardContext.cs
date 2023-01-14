using MovManagerr.Core.Data;
using System.Collections.ObjectModel;

namespace MovManagerr.Cls.UIs
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
