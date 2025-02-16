using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace YourGameServer.Explorer.Controllers;

/// <summary>
/// Controller used in web apps to manage accounts.
/// </summary>
public class AuthController(IHttpContextAccessor httpContextAccessor, ILogger<AuthController> logger) : Controller
{
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<AuthController> _logger = logger;

    public async Task<IActionResult> LogOut()
    {
        var referer = Request.Headers.TryGetValue("Referer", out var stringValues) ? stringValues.ToString() : null;
        _logger.LogInformation("LogOut|User:{User} AuthenticationType:{AuthenticationType} Claims:{Claims} IsAdmin:{IsAdmin} Referer:{Referer}",
            _httpContextAccessor.HttpContext?.User?.Identity?.Name,
            _httpContextAccessor.HttpContext?.User?.Identity?.AuthenticationType,
            _httpContextAccessor.HttpContext?.User?.Claims,
            _httpContextAccessor.HttpContext?.User?.IsInRole("Admin"),
            referer);
        if(referer != null && referer.Contains("Auth/LogOut")) {
            referer = null;
        }
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(referer ?? "/");
    }
}
