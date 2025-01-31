#nullable disable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using YourGameServer.Game.Interface;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;
using PlayerAccountStatus = YourGameServer.Game.Interface.PlayerAccountStatus;

namespace YourGameServer.Game.Services;

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
        return FormalPlayerAccountFromPlayerAccount(playerAccount);
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
                .Where(i => ids.Contains(i.Id) && (PlayerAccountStatus)i.Status < PlayerAccountStatus.Banned).Select(i => MaskedPlayerAccountFromPlayerAccount(i)).ToListAsync();
        }
        return null;
    }

    public static MaskedPlayerAccount MaskedPlayerAccountFromPlayerAccount(PlayerAccount playerAccount) => new()
    {
        Id = playerAccount.Id,
        LastLogin = playerAccount.LastLogin,
        Profile = playerAccount.Profile != null ? MaskedPlayerProfileFromPlayerProfile(playerAccount.Profile) : null
    };

    public static FormalPlayerAccount FormalPlayerAccountFromPlayerAccount(PlayerAccount playerAccount) => new()
    {
        Id = playerAccount.Id,
        Code = playerAccount.Code,
        Status = (PlayerAccountStatus)playerAccount.Status,
        Since = playerAccount.Since,
        LastLogin = playerAccount.LastLogin,
        Profile = playerAccount.Profile != null ? FormalPlayerProfileFromPlayerProfile(playerAccount.Profile) : null
    };

    public static FormalPlayerProfile FormalPlayerProfileFromPlayerProfile(PlayerProfile playerProfile) => new()
    {
        Id = playerProfile.Id,
        OwnerId = playerProfile.OwnerId,
        LastUpdate = playerProfile.LastUpdate,
        Name = playerProfile.Name,
        Motto = playerProfile.Motto,
        IconBlobId = playerProfile.IconBlobId
    };

    public static MaskedPlayerProfile MaskedPlayerProfileFromPlayerProfile(PlayerProfile playerProfile) => new()
    {
        Name = playerProfile.Name,
        Motto = playerProfile.Motto,
        IconBlobId = playerProfile.IconBlobId
    };
}
