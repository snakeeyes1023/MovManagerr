using MovManagerr.Core.Infrastructures.Configurations;

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

        public override void Merge(Entity entity)
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

    public abstract class DownloadableContent : IEquatable<DownloadableContent>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is downloaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is downloaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsDownloaded { get; set; }

        public abstract bool Equals(DownloadableContent? other);
    }

    public abstract class DirectLinkDownload : DownloadableContent
    {
        public string Link { get; set; }

        public virtual string GetExtension()
        {
            return Path.GetExtension(Link);
        }

        public virtual string GetFilename()
        {
            // Récupère le titre et l'extension de l'url
            string title = Path.GetFileNameWithoutExtension(Link);
            string extension = Path.GetExtension(Link);

            // Si aucun titre n'a été trouvé, utilise le titre de TmdbMovie
            if (string.IsNullOrEmpty(title))
            {
                title = Path.GetRandomFileName();
            }

            // Si aucune extension n'a été trouvée, utilise "mp4" par défaut
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".mp4";
            }

            // Retourne le titre et l'extension concaténés
            return $"{title}{extension}";
        }

        public override bool Equals(DownloadableContent? other)
        {
            return other is DirectLinkDownload directLinkDownload 
                && directLinkDownload.Link.Equals(this.Link, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class M3UContentLink : DirectLinkDownload
    {
        public List<string> Tags { get; set; }

    }
}
