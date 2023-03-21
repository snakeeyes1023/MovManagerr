using Hangfire;
using MovManagerr.Core.Infrastructures.Configurations;
using MovManagerr.Core.Infrastructures.Loggers;
using Plex.Api.Factories;

namespace MovManagerr.Core.Helpers.PlexScan
{
    public class PlexScanHelper
    {
        private readonly IPlexFactory _plexFactory;

        private readonly PlexConfiguration _plexConfiguration;

        private static bool InProgress;

        public PlexScanHelper(IPlexFactory plexFactory)
        {
            _plexFactory = plexFactory;
            _plexConfiguration = Preferences.Instance.Settings.PlexConfiguration;
        }

        [Queue("default")]
        public async Task Scan()
        {
            try
            {
                if (!InProgress)
                {
                    InProgress = true;
                    SimpleLogger.AddLog("PLEX : Scan des libraries en cours...", LogType.Info);

                    // or use and Plex Auth token
                    Plex.Library.ApiModels.Accounts.PlexAccount account = _plexFactory
                        .GetPlexAccount(_plexConfiguration.ApiKey);

                    // Get my server
                    var servers = await account.Servers();
                    var myServers = servers?.Where(c => c.Owned == 1);

                    if (myServers != null)
                    {
                        foreach (var server in myServers)
                        {
                            foreach (var library in await server.Libraries())
                            {
                                await library.ScanForNewItems(false);
                            }
                        }
                    }    

                    InProgress = false;
                    SimpleLogger.AddLog("PLEX : Scan des libraries terminé.", LogType.Info);
                }
                else
                {
                    SimpleLogger.AddLog("Scan de la librarie déjà en cours...", LogType.Warning);
                }
            }
            catch (Exception ex)
            {
                InProgress = false;
                throw;
            }
        }
    }
}
