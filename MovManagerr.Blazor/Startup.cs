using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MovManagerr.Core.Services.Movies;
using MovManagerr.Core.Tasks.Backgrounds.ContentTasks;
using MovManagerr.Core.Tasks.Backgrounds.MovieTasks;
using Radzen;

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

            #endregion
            //radzen
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
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
