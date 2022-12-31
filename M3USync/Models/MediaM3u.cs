using m3uParser;
using M3USync.Models.Helpers;
using Sprache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M3USync.Models
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
