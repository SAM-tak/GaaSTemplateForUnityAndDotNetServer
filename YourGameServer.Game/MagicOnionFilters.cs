using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using MagicOnion;
using MagicOnion.Server;
using YourGameServer.Game.Extensions;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;

namespace YourGameServer.Game;

/// <summary>
/// Verify token only
/// </summary>
/// <param name="jwt">Sets by DI</param>
/// <param name="httpContextAccessor">Sets by DI</param>
public class VerifyToken(JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor) : MagicOnionFilterAttribute
{
    readonly JwtAuthorizer _jwt = jwt;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        var authstr = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if(authstr is null || authstr.Length != 2 || !authstr[0].Equals(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)) {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid authorization header.");
        }
        if(!_jwt.ValidateToken(authstr[1])) {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid token.");
        }
        await next(context);
    }
}

/// <summary>
/// Verify token and player accound validation.
/// This cause DB access.
/// </summary>
/// <param name="jwt">Sets by DI</param>
/// <param name="httpContextAccessor">Sets by DI</param>
public class VerifyTokenAndAccount(JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor) : MagicOnionFilterAttribute
{
    readonly JwtAuthorizer _jwt = jwt;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        var authstr = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if(authstr is null || authstr.Length != 2 || !authstr[0].Equals(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)) {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid authorization header.");
        }
        if(!_jwt.ValidateToken(authstr[1])) {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid token.");
        }

        using var scope = context.ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<GameDbContext>();
        if(dbContext != null && _httpContextAccessor.HttpContext != null && _httpContextAccessor.TryGetPlayerIdAndDeviceIdx(out var playerId, out var deviceIdx)) {
            var playerAccount = await dbContext.PlayerAccounts.FirstOrDefaultAsync(x => x.Id == playerId)
                ?? throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Player account is not found.");
            if(playerAccount.CurrentDeviceIdx != deviceIdx) {
                throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Logged in by other device.");
            }
            if(playerAccount.Status >= PlayerAccountStatus.Banned) {
                throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Player account is invalid.");
            }
        }
        else {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Can't verify player account.");
        }

        await next(context);
    }
}
