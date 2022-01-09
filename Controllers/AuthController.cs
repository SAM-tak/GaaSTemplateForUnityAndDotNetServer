using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace YourGameServer.Controllers;

/// <summary>
/// Controller used in web apps to manage accounts.
/// </summary>
public class AuthController : Controller
{
    public async Task<IActionResult> LogOut()
    {
        var referer = Request.Headers.ContainsKey("Referer") ? Request.Headers["Referer"].ToString() : null;
        Console.WriteLine($"Request.Headers['Referer'] = {referer}");
        if(referer != null && referer.Contains("Auth/SignOut")) referer = null;
        //return SignOut(new AuthenticationProperties { RedirectUri = Request.Headers["Referer"] }, "Cookies", "OpenIdConnect");
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(referer ?? "/");
    }
}
