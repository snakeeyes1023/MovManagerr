

using Plex.Api.Factories;
using Plex.Library.Factories;
using Plex.ServerApi;
using Plex.ServerApi.Api;
using Plex.ServerApi.Clients;
using Plex.ServerApi.Clients.Interfaces;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var sectionExplorer = builder.Configuration.GetSection(nameof(MovManagerr.Explorer.Config.ExplorerPathConfig));
var explorerPathConfig = sectionExplorer.Get<MovManagerr.Explorer.Config.ExplorerPathConfig>();
builder.Services.AddSingleton(explorerPathConfig);


var sectionRadarr = builder.Configuration.GetSection(nameof(MovManagerr.Explorer.Config.RadarrInstanceConfig));
var radarrPathConfig = sectionRadarr.Get<MovManagerr.Explorer.Config.RadarrInstanceConfig>();
builder.Services.AddSingleton(radarrPathConfig);


//inject content service class
builder.Services.AddScoped<MovManagerr.Explorer.Services.ContentServices>();

var sectionPlex = builder.Configuration.GetSection("PlexConfig");
var plexPathConfig = sectionRadarr.Get<ClientOptions>();

builder.Services.AddSingleton(plexPathConfig);
builder.Services.AddTransient<IPlexServerClient, PlexServerClient>();
builder.Services.AddTransient<IPlexAccountClient, PlexAccountClient>();
builder.Services.AddTransient<IPlexLibraryClient, PlexLibraryClient>();
builder.Services.AddTransient<IApiService, ApiService>();
builder.Services.AddTransient<IPlexFactory, PlexFactory>();
builder.Services.AddTransient<IPlexRequestsHttpClient, PlexRequestsHttpClient>();

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
