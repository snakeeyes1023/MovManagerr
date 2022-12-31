using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M3USync.Models.Helpers
{
    public static class M3uContentDataHelper
    {
        /// <summary>
        /// Extrait les informations d'un fichier M3U à partir de son contenu 
        /// Exemple : #EXTINF:-1 tvg-id="TF1 HD" tvg-name="TF1 HD" tvg-logo="https://i.imgur.com/1ZQZ1Zm.png" group-title="FRANCE",TF1 HD place tvg-name en cle et TF1 HD en valeur
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static Dictionary<string, string> ExtractValues(string content)
        {
            MatchCollection matches = Regex.Matches(content, "(\\w+)\\s*=\\s*\\\"([^\\\"]*)\\\"");

            //bind matches in infos
            var infos = new Dictionary<string, string>(matches.Count);

            foreach (Match match in matches)
            {
                string propertyName = match.Groups[1].Value;
                string propertyValue = match.Groups[2].Value;

                infos.Add(propertyName, propertyValue);
            }

            return infos;
        }
    }
}
