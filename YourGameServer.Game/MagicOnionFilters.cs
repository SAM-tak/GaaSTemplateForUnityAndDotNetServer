using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Game.Extensions;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;

namespace YourGameServer.Game;

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

public class VerifyTokenAndAccount(JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider) : MagicOnionFilterAttribute
{
    readonly JwtAuthorizer _jwt = jwt;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly IServiceProvider _serviceProvider = serviceProvider;

    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        var authstr = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if(authstr is null || authstr.Length != 2 || !authstr[0].Equals(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)) {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid authorization header.");
        }

        if(!_jwt.ValidateToken(authstr[1])) {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Invalid token.");
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<GameDbContext>();
        if(dbContext != null && _httpContextAccessor.HttpContext != null) {
            if(_httpContextAccessor.HttpContext.TryGetPlayerIdAndDeviceId(out var playerId, out var deviceId)) {
                var playerAccount = await dbContext.PlayerAccounts.FirstOrDefaultAsync(x => x.Id == playerId)
                    ?? throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Player account is not found.");
                if(playerAccount.CurrentDeviceId != deviceId) {
                    throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Logged in by other device.");
                }
                if(playerAccount.Status >= PlayerAccountStatus.Banned) {
                    throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Player account is invalid.");
                }
            }
        }
        await next(context);
    }
}
