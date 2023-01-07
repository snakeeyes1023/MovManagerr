using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace M3USync.Infrastructures.Configurations
{
    public class DirectoryCredsManager : DirectoryManager
    {
        private NetworkCredential _creds;
        private string _server;

        public DirectoryCredsManager(string basePath, string server, NetworkCredential cred) : base(Path.Combine(server, basePath))
        {
            _creds = cred;
            _server = server;
        }

        public override void CreateDirectory(string path)
        {

            throw new NotImplementedException("Pas eu le temp... bug bizarre de auth sur les network drive (tester openmediavault)");

            //NetworkCredential theNetworkCredential = new NetworkCredential(@"jonathan", "plessisville")
            //{
            //    Domain = "workgroup"
            //};
            //CredentialCache theNetCache = new CredentialCache();
            //theNetCache.Add(new Uri(@"\\192.168.0.11"), "NTLM", theNetworkCredential);
            //string[] theFolders = Directory.GetDirectories(@"D:\movie");

            //CredentialCache theNetCache = new CredentialCache();
            //theNetCache.Add(new Uri(_server), "Basic", _creds);
            //string[] theFolders = Directory.GetDirectories(_BasePath);
            //var fullPath = Path.Combine(_BasePath, path);
            //Directory.CreateDirectory(fullPath);
            ////base.CreateDirectory(path);
        }
    }
}
