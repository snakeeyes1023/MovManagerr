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
                .UseLiteDbStorage());


            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "m3u-download", "direct-download", "default" };
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
            services.AddScoped<ContentAddService>();

            services.AddSingleton<IContentDbContext, ContentDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
