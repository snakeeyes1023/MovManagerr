namespace MovManagerr.Web.Infrastructure
{
    public static class Services
    {
        public static void ConfigureSingletonServices<T>(this IServiceCollection service, IConfiguration configuration,  string sectionName) where T : class
        {
            var section = configuration.GetSection(sectionName);
            var config = section.Get<T>();
            service.AddSingleton(config);
        }
    }
}
