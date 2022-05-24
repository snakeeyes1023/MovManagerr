namespace MovManagerr.Web.Infrastructure
{
    public static class Services
    {
        public static void ConfigureSingletonServices<T>(this WebApplicationBuilder builder, string sectionName) where T : class
        {
            var section = builder.Configuration.GetSection(sectionName);
            var config = section.Get<T>();
            builder.Services.AddSingleton(config);
        }
    }
}
