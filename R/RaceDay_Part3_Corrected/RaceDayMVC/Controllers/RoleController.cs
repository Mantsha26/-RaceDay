using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace RaceDayMVC.Controllers;

/// <summary>Base for pages that need a logged-in user with a specific role.</summary>
public abstract class RoleController(string role) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (HttpContext.Session.GetString("ApiToken") == null)
        {
            TempData["Error"] = "Please log in to continue.";
            context.Result = RedirectToAction("Login", "Account");
        }
        else if (!string.Equals(HttpContext.Session.GetString("Role"), role, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = RedirectToAction("Index", "Home");
        }
        base.OnActionExecuting(context);
    }

    protected void Notice(string text) => TempData["Notice"] = text;
    protected void Fail(string text) => TempData["Error"] = text;
}
