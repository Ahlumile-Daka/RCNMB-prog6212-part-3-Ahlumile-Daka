using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Mvc.Helpers;

namespace RaceDay.Mvc.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeSessionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[]? _roles;

    public AuthorizeSessionAttribute(params string[] roles)
    {
        _roles = roles?.Length > 0 ? roles : null;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!SessionHelper.IsAuthenticated(context.HttpContext))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = context.HttpContext.Request.Path });
            return;
        }

        if (_roles != null && _roles.Length > 0)
        {
            var role = SessionHelper.GetRole(context.HttpContext);
            if (role == null || !_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            }
        }
    }
}
