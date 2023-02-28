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
            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseLiteDbStorage());

            
            services.AddHangfireServer();

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.ConfigureSingletonServices<Tmdb.Config.TmdbConfig>(Configuration, "TmdbConfig");


            services.AddScoped<Tmdb.TmdbClientService>();
            services.AddScoped<Tmdb.Service.FavoriteService>();

            #region MyServices

            services.AddScoped<IMovieService, MovieService>();

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
                CreateWindow();
            }
        }

        private async void CreateWindow()
        {
            var window = await Electron.WindowManager.CreateWindowAsync();

            // put in full page
            var display = await Electron.Screen.GetPrimaryDisplayAsync();
            var size = display.WorkAreaSize;
            window.SetBounds(new Rectangle() { X = 0, Y = 0, Width = size.Width, Height = size.Height });

            // set the window title
            window.SetTitle("MovManagerr");

            // prevent quit app
            Electron.App.On("window-all-closed", () => { });

            //// initialise the menu
            //var menu = new MenuItem[]
            //{
            //    new MenuItem
            //    {
            //        Label = "File",
            //        Submenu = new MenuItem[]
            //        {
            //            new MenuItem
            //            {
            //                Label = "Exit",
            //                Click = () => Electron.App.Quit()
            //            }
            //        }
            //    },
            //    new MenuItem
            //    {
            //        Label = "Edit",
            //        Submenu = new MenuItem[]
            //        {
            //            new MenuItem
            //            {
            //                Label = "Undo",
            //                Role = MenuRole.undo
            //            },
            //            new MenuItem
            //            {
            //                Label = "Redo",
            //                Role = MenuRole.redo
            //            },
            //            new MenuItem
            //            {
            //                Type = MenuType.separator
            //            },
            //            new MenuItem
            //            {
            //                Label = "Cut",
            //                Role = MenuRole.cut
            //            },
            //            new MenuItem
            //            {
            //                Label = "Copy",
            //                Role = MenuRole.copy
            //            },
            //            new MenuItem
            //            {
            //                Label = "Paste",
            //                Role = MenuRole.paste
            //            },
            //        }
            //    },
            //    new MenuItem
            //    {
            //        Label = "Window",
            //        Submenu = new MenuItem[]
            //        {
            //            new MenuItem
            //            {
            //                Label = "Minimize",
            //                Role = MenuRole.minimize
            //            },
            //            new MenuItem
            //            {
            //                Label = "Close",
            //                Role = MenuRole.minimize
            //            }
            //        }
            //    },
            //    new MenuItem
            //    {
            //        Label = "Help",
            //        Submenu = new MenuItem[]
            //        {
            //            new MenuItem
            //            {
            //                Label = "Learn More",
            //                Click = async () => await Electron.Shell.OpenExternalAsync("")
            //                }
            //            }
            //        }
            //    };

            //Electron.Menu.SetApplicationMenu(menu);

            window.OnClosed += () =>
            {
                Electron.App.Quit();
            };
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
