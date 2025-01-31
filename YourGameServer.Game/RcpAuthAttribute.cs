using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace YourGameServer.Game;

public class RpcAuthAttribute(JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor) : MagicOnionFilterAttribute
{
    readonly JwtAuthorizer _jwt = jwt;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        var authstr = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if(authstr is null || authstr.Length != 2 || !authstr[0].Equals(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase) || !_jwt.ValidateToken(authstr[1])) {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "Unauthenticated");
        }
        await next(context);
    }
}
