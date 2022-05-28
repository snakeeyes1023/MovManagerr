using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MovManagerr.Web.Infrastructure
{
    public class AdminActionFilter : IActionFilter
    {

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("username") == null)
            {
                context.Result = new RedirectResult("/Account/Login");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.
        }
    }
}
