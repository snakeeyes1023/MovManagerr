using MovManagerr.Core.Data.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.Configurations.ContentPreferences
{
    public abstract class ContentPreference<T> : IContentPreference where T : Content
    {
        public abstract string BasePath { get; set; }

        public Dictionary<int, string> GenresPath { get; protected set; }
        public string SectionName { get; set; }

        public ContentPreference()
        {
            GenresPath = new Dictionary<int, string>();
            SectionName = $"Configuration : {typeof(T).Name}";
        }


        public string GetFolderForGenre(int genreId)
        {
            try
            {
                return GenresPath[genreId];
            }
            catch (Exception)
            {
                return "Autres";
            }
        }

        public DirectoryManager GetDirectoryManager()
        {
            return new DirectoryManager(BasePath);

            //else
            //{
            //    string server = Configs.GetValueOrDefault(searchManager + "_server", string.Empty);

            //    if (!string.IsNullOrEmpty(server))
            //    {
            //        string user = Configs.GetValueOrDefault(searchManager + "_user", string.Empty);
            //        string password = Configs.GetValueOrDefault(searchManager + "_pass", string.Empty);

            //        NetworkCredential cred = new NetworkCredential(user, password);
            //        return new DirectoryCredsManager(path, server, cred);
            //    }

            //    return new DirectoryManager(path);
            //}
        }
    }
}
