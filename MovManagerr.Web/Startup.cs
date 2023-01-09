using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MovManagerr.Web.Infrastructure;
using Plex.ServerApi.PlexModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovManagerr.Web
    
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
            // Add services to the container.
            services.ConfigureSingletonServices<MovManagerr.Explorer.Config.ExplorerPathConfig>(Configuration, "ExplorerPathConfig");
            services.ConfigureSingletonServices<MovManagerr.Explorer.Config.RadarrInstanceConfig>(Configuration, "RadarrInstanceConfig");
            services.ConfigureSingletonServices<MovManagerr.Explorer.Config.FtpConfig>(Configuration, "FtpConfig");
            services.ConfigureSingletonServices<MovManagerr.Tmdb.Config.TmdbConfig>(Configuration, "TmdbConfig");

            //inject content service class
            services.AddScoped<MovManagerr.Tmdb.TmdbClientService>();
            services.AddScoped<MovManagerr.Tmdb.Service.FavoriteService>();
            services.AddScoped<MovManagerr.Explorer.Services.ContentServices>();
            services.AddScoped<MovManagerr.Explorer.Services.MovieServices>();
            services.AddScoped<AdminActionFilter>();


            services.AddHttpContextAccessor();
            services.TryAddScoped<IActionContextAccessor, ActionContextAccessor>();


            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(5);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services
                .AddControllersWithViews()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
