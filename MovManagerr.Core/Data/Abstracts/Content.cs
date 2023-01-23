using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using Snake.LiteDb.Extensions.Models;
using System.Net;

namespace MovManagerr.Core.Data.Abstracts
{
    public abstract class Content : Entity
    {
        public Content()
        {
            DownloadableContents = new List<DownloadableContent>();
        }

        public Content(string name, string poster)
        {
            Name = name;
            Poster = poster;
            DownloadableContents = new List<DownloadableContent>();
        }

        public string Name { get; set; }

        public string Poster { get; set; }

        public List<DownloadableContent> DownloadableContents { get; protected set; }

        public Dictionary<string, object> CustomData { get; protected set; }

        #region Methods

        public abstract string GetDirectoryPath();

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

            downloadLink.StartDownload(serviceProvider, this);
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
