using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovManagerr.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.ConfigureSingletonServices<MovManagerr.Explorer.Config.ExplorerPathConfig>("ExplorerPathConfig");
builder.ConfigureSingletonServices<MovManagerr.Explorer.Config.RadarrInstanceConfig>("RadarrInstanceConfig");
builder.ConfigureSingletonServices<MovManagerr.Explorer.Config.FtpConfig>("FtpConfig");
builder.ConfigureSingletonServices<MovManagerr.Tmdb.Config.TmdbConfig>("TmdbConfig");

//inject content service class
builder.Services.AddScoped<MovManagerr.Tmdb.TmdbClientService>();
builder.Services.AddScoped<MovManagerr.Tmdb.Service.FavoriteService>();
builder.Services.AddScoped<MovManagerr.Explorer.Services.ContentServices>();
builder.Services.AddScoped<MovManagerr.Explorer.Services.MovieServices>();
builder.Services.AddScoped<AdminActionFilter>();

#region Setup Hangfire

builder.Services.AddHangfire(configuration => configuration
      .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
      .UseSimpleAssemblyNameTypeSerializer()
      .UseRecommendedSerializerSettings()
      .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
      {
          CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
          SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
          QueuePollInterval = TimeSpan.Zero,
          UseRecommendedIsolationLevel = true,
          DisableGlobalLocks = true
      }));

builder.Services.AddHangfireServer();

builder.Services.AddHttpContextAccessor();
builder.Services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

#endregion

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);


var app = builder.Build();


app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Jobs",
    Authorization = new[]
    {
       new  HangfireAuthorizationFilter("admin")
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard();
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

RecurringJob.AddOrUpdate<MovManagerr.Tmdb.Service.FavoriteService>("addMovies", x => x.LikeNewMovieAsync(2), Cron.Daily);
RecurringJob.AddOrUpdate<MovManagerr.Explorer.Services.MovieServices>("syncMovies", x => x.SyncMovieListByFolderAsync(), Cron.Daily);
RecurringJob.AddOrUpdate<MovManagerr.Explorer.Services.MovieServices>("deleteBads", x => x.DeleteBadMovie(), Cron.Daily);

app.Run();

