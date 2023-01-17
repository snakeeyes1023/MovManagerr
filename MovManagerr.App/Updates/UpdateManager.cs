using System.Net;
using System.IO.Compression;

namespace MovManagerr.App.Updates
{
    public class UpdateManager
    {
        private string _currentVersion;
        private string _latestVersion;
        private string _urlVersionProvider;
        private string _urlDownloadProvider;

        public UpdateManager(string urlVersionProvider = "", string urlDownloadProvider = "https://github.com/snakeeyes1023/MovManagerr/releases/download/Beta/MovManagerr.master.0.0.1.windows-core-x64.zip")
        {
            _currentVersion = "0.0.1";
            _urlVersionProvider = urlVersionProvider;
            _urlDownloadProvider = urlDownloadProvider;
        }

        public bool IsUpdateAvailable()
        {
            return _currentVersion != GetLatestVersion();
        }

        public void DownloadUpdate(string versionToDownload)
        {
            // Code to download update from specified url and install it
            try
            {
                using (WebClient client = new WebClient())
                {
                    string url = _urlDownloadProvider /*+ versionToDownload*/;
                    string destination = Path.GetTempPath() + versionToDownload + ".zip";
                    client.DownloadFile(url, destination);
                    //unzip and install the update 
                    string extractPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/web";
                    ZipFile.ExtractToDirectory(destination, extractPath);
                }
            }
            catch (Exception e)
            {
                // handle the exception 
                Console.WriteLine("An error occurred while downloading the update: " + e.Message);
            }
        }

        public string GetLatestVersion()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    return "0.0.2";
                    return client.DownloadString(_urlVersionProvider);
                }
            }
            catch (Exception)
            {
                return _latestVersion;
            }
        }
    }
}