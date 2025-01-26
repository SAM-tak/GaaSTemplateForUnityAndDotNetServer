#nullable disable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using YourGameServer.Data;
using YourGameServer.Interface;
using YourGameServer.Models;

namespace YourGameServer.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class PlayerAccountService(GameDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger)
    : ServiceBase<IPlayerAccountService>, IPlayerAccountService
{
    readonly GameDbContext _context = context;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<AccountService> _logger = logger;

    [FromTypeFilter(typeof(RpcAuthAttribute))]
    public async UnaryResult<FormalPlayerAccount> GetPlayerAccount()
    {
        ulong playerId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["playerid"]);
        var playerAccount = await _context.PlayerAccounts.FindAsync(playerId)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        return playerAccount.MakeFormal();
    }

    [FromTypeFilter(typeof(RpcAuthAttribute))]
    public async UnaryResult<IEnumerable<MaskedPlayerAccount>> GetPlayerAccounts(ulong[] ids)
    {
        ulong playerId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["playerid"]);
        _logger.LogInformation("{PlayerId} {Request}", playerId, ids.ToJson());
        if(!await _context.PlayerAccounts.AnyAsync()) {
            throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        }

        if(ids != null && ids.Length > 0) {
            return await _context.PlayerAccounts.Include(i => i.Profile)
                .Where(i => ids.Contains(i.Id) && i.Status < PlayerAccountStatus.Banned).Select(i => i.MakeMasked()).ToListAsync();
        }
        return null;
    }
}
