using MovManagerr.Core.Data.Enums;
using MovManagerr.Core.Downloaders.M3U;
using MovManagerr.Core.Infrastructures.Configurations;

namespace MovManagerr.Core.Data.Abstracts
{
    public abstract class Content : Entity
    {
        public Content(MediaM3u media)
        {
            Name = ExtractName(media.MuName);

            Tags = media.MuName.Split("|")
                .ToList()
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .Select(x => x.Trim())
                .ToList();

            Tags.RemoveAt(Tags.Count - 1);

            Poster = media.MuLogo;
            Url = media.MuUrl;
        }

        public Content()
        {

        }

        public virtual string ExtractName(string muName)
        {
            return (muName.Split("|").Last() ?? "").Trim();
        }
        public string Size { get; set; }
        public string Url { get; set; }
        public ContentType Type { get; set; }
        public string TMDBID { get; set; }
        public string Name { get; set; }
        public string Poster { get; set; }
        public List<string> Tags { get; protected set; }
        public abstract bool Equals(Content content);

        public abstract string GetDirectoryPath();

        public abstract string GetFileName();

        public abstract DirectoryManager GetDirectoryManager();

        public string GetFullPath()
        {
            return Path.Combine(GetDirectoryManager()._BasePath, GetDirectoryPath(), GetFileName());
        }

        public string GetExtension()
        {
            return Path.GetExtension(Url);
        }

        public override string ToString()
        {
            var tags = string.Join("|", Tags);
            return $"{Name} - {Url} | {tags}";
        }
    }
}
