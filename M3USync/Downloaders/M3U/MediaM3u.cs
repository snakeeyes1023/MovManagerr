using M3USync.Data.Helpers;

namespace M3USync.Downloaders.M3U
{
    public class MediaM3u
    {
        public MediaM3u()
        {
            Infos = new Dictionary<string, string>();
        }


        public MediaM3u(string content)
        {
            MuFullContent = content;
            Infos = M3uContentDataHelper.ExtractValues(content);
        }


        private Dictionary<string, string> Infos;

        public readonly string MuFullContent;


        public string MuID { get => Infos["id"]; set => Infos["id"] = value; }
        public string MuName { get => Infos["name"]; set => Infos["name"] = value; }
        public string MuLogo { get => Infos["logo"]; set => Infos["logo"] = value; }
        public string MuTitle { get => Infos["title"]; set => Infos["title"] = value; }
        public string MuUrl { get; private set; }

        public void SetUrl(string url)
        {
            MuUrl = url;
        }
    }
}
