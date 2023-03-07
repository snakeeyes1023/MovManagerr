using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MovManagerr.Core.Downloaders.Contents;
using MovManagerr.Core.Infrastructures.Dbs;
using MovManagerr.Core.Services.Movies;
using MovManagerr.Core.Tasks.Backgrounds.ContentTasks;
using MovManagerr.Core.Tasks.Backgrounds.MovieTasks;
using Radzen;
using ElectronNET.API;
using System;
using MovManagerr.Core.Tasks;
using ElectronNET.API.Entities;
using System.Linq;
using System.Diagnostics;
using Hangfire;
using Hangfire.LiteDB;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Plex.Api.Factories;
using Plex.Library.Factories;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.Clients;
using Plex.ServerApi;
using Plex.ServerApi.PlexModels.Media;
using MovManagerr.Core.Infrastructures.Configurations;
using Microsoft.Extensions.Logging;
using MovManagerr.Core.Infrastructures.Loggers;
using MovManagerr.Core.Importers;

namespace MovManagerr.Blazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            GlobalConfiguration.Configuration.UseSerializerSettings(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseLiteDbStorage(Preferences.Instance._HangFireDbPath));

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 2 });

            services.AddHangfireServer(options =>
            {
                options.ServerName = String.Format("{0}:filemanagement", Environment.MachineName);
                options.Queues = new[] { "file-transfert" };
                options.WorkerCount= 4;
            });


            services.AddHangfireServer(options =>
            {
                options.ServerName = String.Format("{0}:transcoder", Environment.MachineName);
                options.Queues = new[] { "transcode" };
                options.WorkerCount = 1;
            });

            services.AddHangfireServer(options =>
            {
                options.ServerName = String.Format("{0}:normal", Environment.MachineName);
                options.Queues = new[] { "default" };
                options.WorkerCount = 4;
            });

            services.AddHangfireServer(options =>
            {
                options.ServerName = String.Format("{0}:downloader", Environment.MachineName);
                options.Queues = new[] { "m3u-download", "direct-download" };
                options.WorkerCount = 1;
            });

            services.AddHangfireServer(options =>
            {
                options.ServerName = String.Format("{0}:cronjob", Environment.MachineName);
                options.Queues = new[] { "sync-task"};
                options.WorkerCount = 1;
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.ConfigureSingletonServices<Tmdb.Config.TmdbConfig>(Configuration, "TmdbConfig");


            services.AddScoped<Tmdb.TmdbClientService>();
            services.AddScoped<Tmdb.Service.FavoriteService>();

            #region MyServices

            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IDownloadedMovieService, DownloadedMovieService>();
            #endregion

            #region PLEX Api
            
            var apiOptions = new ClientOptions
            {
                Product = "API_UnitTests",
                DeviceName = "API_UnitTests",
                ClientId = "MyClientId",
                Platform = "Web",
                Version = "v1"
            };

            services.AddSingleton(apiOptions);
            services.AddTransient<IPlexServerClient, PlexServerClient>();
            services.AddTransient<IPlexAccountClient, PlexAccountClient>();
            services.AddTransient<IPlexLibraryClient, PlexLibraryClient>();
            services.AddTransient<IApiService, ApiService>();
            services.AddTransient<IPlexFactory, PlexFactory>();
            services.AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>();

            #endregion

            #region BackgroundService

            services.AddSingleton<SearchAllMoviesOnTmdb>();
            services.AddSingleton<SyncM3UFiles>();
            services.AddSingleton<ContentDownloaderClient>();

            #endregion
            //radzen
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();
            services.AddScoped<ImportContentService>();
            services.AddScoped<PlexImporter>();

            services.AddSingleton<IContentDbContext, ContentDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AddMediaInfoToEnvVariable();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHangfireDashboard();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHangfireDashboard();
            });

            if (HybridSupport.IsElectronActive)
            {
                Task.Run(async () =>
                {
                    var mainWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
                    {
                        Width = 1152,
                        Height = 940,
                        DarkTheme = true,
                        Show = false
                    });

                    mainWindow.SetTitle("MovManagerr");

                    await mainWindow.WebContents.Session.ClearCacheAsync();

                    mainWindow.OnReadyToShow += () => mainWindow.Show();

                    Electron.IpcMain.On("hideToSystemTray", (e) =>
                    {
                        mainWindow.Hide();

                        if (Electron.Tray.MenuItems.Count == 0)
                        {
                            var menu = new MenuItem[]
                            {
                                new MenuItem
                                {
                                    Label = "Ouvrir la fenêtre",
                                    Click = () => mainWindow.Show()
                                },
                                new MenuItem
                                {
                                    Label = "Quitter",
                                    Click = () => Electron.App.Exit()
                                }
                            };

                            Electron.Tray.Show(@"/resources/bin/wwwroot/assets/icons/icon.png", menu);
                            Electron.Tray.SetToolTip("Movmanager - Gestion de contenu multimédia.");
                        }
                    });
                });
            }
        }

        private static void AddMediaInfoToEnvVariable()
        {
            try
            {
                string pathVariable = Environment.GetEnvironmentVariable("PATH");
                if (!pathVariable.Contains("C:\\Program Files\\MediaInfo"))
                {
                    // Ajouter le nouveau chemin au début de la variable d'environnement PATH
                    string newPath = "C:\\Program Files\\MediaInfo" + ";" + pathVariable;
                    Environment.SetEnvironmentVariable("PATH", newPath);
                }
            }
            catch (Exception)
            {
                SimpleLogger.AddLog("Impossible de défénir le PATH the media info", LogType.Error);
            }
        }
    }


    public static class StartupExtensions
    {
        public static void ConfigureSingletonServices<T>(this IServiceCollection service, IConfiguration configuration, string sectionName) where T : class
        {
            var section = configuration.GetSection(sectionName);
            service.Configure<T>(section);
        }
    }
}
