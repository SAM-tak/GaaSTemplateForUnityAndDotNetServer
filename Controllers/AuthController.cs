using System;
using System.Threading.Tasks;
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
    public new async Task<IActionResult> SignOut()
    {
        var referer = Request.Headers["Referer"].ToString();
        Console.WriteLine($"Request.Headers['Referer'] = {referer}");
        //return SignOut(new AuthenticationProperties { RedirectUri = Request.Headers["Referer"] }, "Cookies", "OpenIdConnect");
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(referer ?? "/");
    }
}