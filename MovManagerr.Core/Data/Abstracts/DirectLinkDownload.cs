namespace MovManagerr.Core.Data.Abstracts
{
    public abstract class DirectLinkDownload : DownloadableContent
    {
        public virtual string Link { get; set; }

        public override string Size
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Size))
                {
                    //// make a get request to get the size
                    //var request = WebRequest.Create(Link);
                    //request.Method = "HEAD";
                    //using var response = request.GetResponse();
                    //Size = response.Headers.Get("Content-Length");

                    if (string.IsNullOrWhiteSpace(base.Size))
                    {
                        Size = "Unknown";
                    }
                }
             
                return base.Size;
            }
            set => base.Size = value;
        }

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
}
