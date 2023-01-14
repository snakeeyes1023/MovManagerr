using M3USync.UIs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.UIs
{
    public abstract class CardContext
    {
        private readonly Collection<ICardConfiguration> Configurations;
        
        public CardContext()
        {
            Configurations = new Collection<ICardConfiguration>();
            Configure();
        }


        public string PrintHtml(object value) 
        {
            var config = GetConfiguration(value.GetType());
            return string.Empty;
        }
        
        private ICardConfiguration GetConfiguration(Type type)
        {
            if (Configurations.FirstOrDefault(c => c.Type == type) is ICardConfiguration configuration)
            {
                return configuration;
            }
            throw new InvalidOperationException("Aucune configuration créé pour le type " + nameof(type));
        }
        
        private void Configure()
        {
            ApplyConfiguration(Configurations);
        }

        public abstract void ApplyConfiguration(Collection<ICardConfiguration> configs);
    }



/// <summary>
/// Save all field map to the card
/// </summary>
    public class CardConfiguration<T> : ICardConfiguration
    {
        public Type Type { get; set; }
        public string? TitleMap  { get; set; }
        public string? DescriptionMap { get; set; }


        public void SetTitle(Func<T, string> titleSelector)
        {
            if (titleSelector is null)
            {
                throw new ArgumentNullException(nameof(titleSelector));
            }

            var propertyName = nameof(titleSelector);
            TitleMap = propertyName;
        }

    }
}
