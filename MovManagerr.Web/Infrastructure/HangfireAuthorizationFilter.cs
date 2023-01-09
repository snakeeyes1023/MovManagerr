//using Hangfire.Annotations;
//using Hangfire.Dashboard;

//public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
//{
//    private readonly string[] _roles;

//    public HangfireAuthorizationFilter(params string[] roles)
//    {
//        _roles = roles;
//    }

//    public bool Authorize([NotNull] DashboardContext context)
//    {
//        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
//        {
//            return true;
//        }
        
//        var coreContext = context.GetHttpContext();
//        var session = coreContext.Session;

//        if (session != null & session.GetString("username") != null)
//        {
//            return true;
//        }

//        return false;
//    }
//}