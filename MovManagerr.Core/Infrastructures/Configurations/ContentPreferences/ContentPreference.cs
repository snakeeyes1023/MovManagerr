using MovManagerr.Core.Data.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Infrastructures.Configurations.ContentPreferences
{
    public abstract class ContentPreference : IContentPreference
    {
        public abstract string BasePath { get; set; }

        public virtual Dictionary<int, string> GenresPath { get; protected set; } = new Dictionary<int, string>();
        public virtual string UnfoudedGenreFolder { get; set; } = "Autres";

        public virtual string SectionName { get; set; }

        //public ContentPreference()
        //{
        //    SectionName = $"Configuration : {GetType().Name}";
        //}


        public string GetFolderForGenre(int genreId)
        {
            try
            {
                return GenresPath[genreId];
            }
            catch (Exception)
            {
                return UnfoudedGenreFolder;
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
