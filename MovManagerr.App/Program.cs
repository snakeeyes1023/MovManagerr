using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MovManagerr.App
{
    internal static class Program
    {
        public static event Action<bool> OnWebServerStatusChanged;
        private static Thread webHostThread;
        private static IHost webHost;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Application.Run(new Main());
        }

        public static void StopWebHost()
        {
            if (IsWebHostStated())
            {
                // Stop the web host
                webHost.StopAsync().Wait();
                webHostThread.Join();
                OnWebServerStatusChanged?.Invoke(false);
            }
        }

        public static void StartWebHost()
        {
            if (!IsWebHostStated())
            {
                // Start the web host on a new thread
                webHost = Web.Program.CreateHostBuilder(Array.Empty<string>()).Build();
                webHostThread = new Thread(() => webHost.Run());
                webHostThread.Start();
                OnWebServerStatusChanged?.Invoke(true);
            }
        }

        public static bool IsWebHostStated()
        {
            return webHostThread != null && webHostThread.IsAlive;
        }
    }
}