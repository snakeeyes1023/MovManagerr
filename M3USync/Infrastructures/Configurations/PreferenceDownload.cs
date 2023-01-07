using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Infrastructures.Configurations
{
    /// <summary>
    /// Les heures de téléchargement de contenue (de 0 à 23)
    /// </summary>
    public class PreferenceDownload
    {
        public PreferenceDownload(string value)
        {
            try
            {
                var values = value.Split(",");
                FromHour = int.Parse(values[0]);
                ToHour = int.Parse(values[1]);

                IsValid = true;
            }
            catch (Exception)
            {
                IsValid = false;
            }
        }

        public bool IsValid { get; set; }
        public int FromHour { get; set; }
        public int ToHour { get; set; }

        /// <summary>
        /// Valide si l'heure actuelle est comprise entre les heures de téléchargement
        /// </summary>
        /// <returns></returns>
        public bool CanDownload()
        {
            var currentHour = DateTime.Now.Hour;

            return currentHour > FromHour && currentHour < ToHour;
        }
    }
}
