using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var userRole = session.GetString("UserRole");

            if (!userId.HasValue || string.IsNullOrEmpty(userRole))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authentication required." });
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new { message = "Access forbidden for this role." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
