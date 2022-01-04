using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using YourGameServer.Data;

namespace YourGameServer;

internal class ApiAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    readonly GameDbContext _context;

    public ApiAuthHandler(GameDbContext context, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _context = context;
    }

    async Task<bool> IsValidRequest()
    {
        if(Request.Headers.ContainsKey(HeaderNames.Authorization)) {
            Logger.LogInformation("Authorization = {authorization}", Request.Headers[HeaderNames.Authorization].FirstOrDefault());
            if(int.TryParse(Request.Headers[HeaderNames.Authorization].FirstOrDefault(), out int id)) {
                Logger.LogInformation("id = {id}", id);
                return await _context.PlayerAccounts.AnyAsync(i => i.Id == id);
            }
        }
        return false;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if(await IsValidRequest()) {
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(Scheme.Name)), Scheme.Name));
        }
        return AuthenticateResult.Fail("Unauthenticated");
    }
}

internal class ApiSelfOnlyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    readonly GameDbContext _context;

    public ApiSelfOnlyAuthHandler(GameDbContext context, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _context = context;
    }

    async Task<bool> IsValidRequest()
    {
        if(Request.Headers.ContainsKey(HeaderNames.Authorization)) {
            Logger.LogInformation("Authorization = {authorization}", Request.Headers[HeaderNames.Authorization].FirstOrDefault());
            if(int.TryParse(Request.Headers[HeaderNames.Authorization].FirstOrDefault(), out int id)) {
                Logger.LogInformation("id = {id}", id);
                return await _context.PlayerAccounts.AnyAsync(i => i.Id == id);
            }
        }
        return false;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if(await IsValidRequest()) {
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(Scheme.Name)), Scheme.Name));
        }
        return AuthenticateResult.Fail("Unauthenticated");
    }
}

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
internal class ApiPlayerSelfOnlyAttribute : Attribute, IAsyncActionFilter
{
    private const string APIKEYNAME = "ApiKey";
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var extractedApiKey))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "Api Key was not provided"
            };
            return;
        }

        var appSettings = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        var apiKey = appSettings.GetValue<string>(APIKEYNAME);

        if (!apiKey.Equals(extractedApiKey))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "Api Key is not valid"
            };
            return;
        }

        await next();
    }
}

internal class ApiAuthAttribute : AuthorizeAttribute
{
    public ApiAuthAttribute()
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }

    public ApiAuthAttribute(string policy) : base(policy)
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }
}

internal class AllowOtherPlayerAttribute : AuthorizeAttribute
{
    public AllowOtherPlayerAttribute() : base("AllowOtherPlayer")
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }
}
