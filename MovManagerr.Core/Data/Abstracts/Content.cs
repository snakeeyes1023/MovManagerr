using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using Snake.LiteDb.Extensions.Models;
using System.Net;
using TMDbLib.Objects.Movies;

namespace MovManagerr.Core.Data.Abstracts
{
    public abstract class Content : Entity
    {
        public Content()
        {
            DownloadableContents = new List<DownloadableContent>();
            DownloadedContents = new List<DownloadedContent>();
        }

        public Content(string name, string poster)
        {
            Name = name;
            Poster = poster;
            DownloadableContents = new List<DownloadableContent>();
            DownloadedContents = new List<DownloadedContent>();
        }

        public string Name { get; set; }

        public string Poster { get; set; }



        public string GetCorrectedPoster()
        {
            if (Poster != null && !Poster.StartsWith("http"))
            {
                return "https://image.tmdb.org/t/p/w200/" + Poster;

            }
            return Poster ?? string.Empty;
        }

        public List<DownloadedContent> DownloadedContents { get; protected set; }

        public List<DownloadableContent> DownloadableContents { get; protected set; }

        public Dictionary<string, object> CustomData { get; protected set; }

        public bool IsDownloaded
        {
            get
            {
                return DownloadedContents.Count > 0;
            }
        }

        public decimal MaxBitrate
        {
            get
            {
                return this.GetMaxBitrate();
            }
        }

        public int NbFiles
        {
            get
            {
                return this.DownloadedContents.Count;
            }
        }

        #region Methods

        /// <summary>
        /// Retourne le chemin du dossier de téléchargement du contenu
        /// </summary>
        /// <returns></returns>
        public virtual string GetPath(bool createDirectory = true)
        {
            var path = GetDirectoryManager()._BasePath;

            if (createDirectory)
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual string GetFullPath(string fileName, bool createDirectory = true)
        {
            return Path.Combine(GetPath(createDirectory), fileName);
        }

        /// <summary>
        /// Représente l'emplacement ou son stocker les contenues du même type (exemple : les films dans le dossier "Films")
        /// </summary>
        /// <returns></returns>
        public abstract DirectoryManager GetDirectoryManager();

        public void AddDownloadableContent(DownloadableContent downloable)
        {
            if (DownloadableContents.Any(x => downloable.Equals(x)))
            {
                return;
            }

            DownloadableContents.Add(downloable);
        }

        public void AddDownloadableContent(IEnumerable<DownloadableContent> downloads)
        {
            foreach (var downloable in downloads)
            {
                AddDownloadableContent(downloable);
            }
        }

        public void Download(IServiceProvider serviceProvider, DownloadableContent? downloadLink = null)
        {
            if (downloadLink == null)
            {
                downloadLink = DownloadableContents.FirstOrDefault();

                if (downloadLink == null)
                {
                    SimpleLogger.AddLog("Aucun fichier trouver", LogType.Error);
                    return;
                }
            }

            downloadLink.Download(serviceProvider, this);
        }

        public void AddCustomData(string key, object data)
        {
            if (CustomData == null)
            {
                CustomData = new Dictionary<string, object>();
            }

            CustomData.Add(key, data);
        }

        public T? GetCustomData<T>(string key)
        {
            if (CustomData != null && CustomData.GetValueOrDefault(key) is T data)
            {
                return data;
            }

            return default;
        }

        public abstract bool Equals(Content content);

        public string[] GetCombinedTags()
        {
            return DownloadableContents
                     .OfType<M3UContentLink>()
                     .SelectMany(x => x.Tags)
                     .Distinct()
                     .ToArray();
        }

        public virtual void Merge(Entity entity)
        {
            if (entity is Content content)
            {
                AddDownloadableContent(content.DownloadableContents);

                Poster = content.Poster;

                if (CustomData == null)
                {
                    CustomData = content.CustomData;
                }
                else
                {
                    foreach (var item in content.CustomData)
                    {
                        if (!CustomData.ContainsKey(item.Key))
                        {
                            CustomData.Add(item.Key, item.Value);
                        }
                        else
                        {
                            CustomData[item.Key] = item.Value;
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot merge entity with different type");
            }
        }
        #endregion
    }
}
