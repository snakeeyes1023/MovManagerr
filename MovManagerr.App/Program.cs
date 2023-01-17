using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace MovManagerr.App
{
    internal static class Program
    {
        public static event Action<bool> OnWebServerStatusChanged;
        private static Process webHostThread;
        private static IHost webHost;

        private const string executablePath = "/web/MovManagerr.Blazor.exe";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Main());
        }

        public static void StopWebHost()
        {
            if (IsWebHostStated())
            {
                webHostThread.Kill();
                OnWebServerStatusChanged?.Invoke(false);
            }
        }

        public static void StartWebHost()
        {
            if (!IsWebHostStated())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + executablePath,
                    CreateNoWindow = true
                };

                webHostThread = Process.Start(startInfo) ?? throw new InvalidOperationException("Impossible de lancer le serveur");

                OnWebServerStatusChanged?.Invoke(true);
            }
        }

        public static bool IsWebHostStated()
        {
            return webHostThread != null && !webHostThread.HasExited;
        }
    }
}