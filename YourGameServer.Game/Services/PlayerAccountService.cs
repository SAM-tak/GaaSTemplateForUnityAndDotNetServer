#nullable disable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Game.Extensions;
using YourGameServer.Game.Interface;
using YourGameServer.Shared;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;
using PlayerAccountStatus = YourGameServer.Game.Interface.PlayerAccountStatus;

namespace YourGameServer.Game.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
[FromTypeFilter(typeof(VerifyTokenAndAccount))]
public class PlayerAccountService(GameDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger)
    : ServiceBase<IPlayerAccountService>, IPlayerAccountService
{
    readonly GameDbContext _dbContext = dbContext;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<AccountService> _logger = logger;

    public async UnaryResult<FormalPlayerAccount> GetPlayerAccount()
    {
        var playerId = _httpContextAccessor.GetPlayerId();
        var playerAccount = await _dbContext.PlayerAccounts.FindAsync(playerId)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        return FormalPlayerAccountFromPlayerAccount(playerAccount);
    }

    public async UnaryResult<IEnumerable<MaskedPlayerAccount>> GetPlayerAccounts(string[] codes)
    {
        var playerId = _httpContextAccessor.GetPlayerId();
        _logger.LogInformation("{PlayerId}|GetPlayerAccounts {Request}", playerId, codes);
        if(!await _dbContext.PlayerAccounts.AnyAsync()) {
            throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        }
        if(codes == null) {
            throw new ReturnStatusException(StatusCode.InvalidArgument, "codes is null.");
        }

        var ids = codes.Select(x => IDCoder.Decode(x).Item1).ToArray();

        if(ids != null && ids.Length > 0) {
            return await _dbContext.PlayerAccounts.Include(i => i.Profile)
                .Where(i => ids.Contains(i.Id) && (PlayerAccountStatus)i.Status < PlayerAccountStatus.Banned)
                .Select(i => MaskedPlayerAccountFromPlayerAccount(i)).ToListAsync();
        }
        return null;
    }

    public async UnaryResult<IEnumerable<MaskedPlayerAccount>> FindPlayerAccounts(int maxCount)
    {
        var playerId = _httpContextAccessor.GetPlayerId();
        _logger.LogInformation("{PlayerId}|FindPlayerAccounts {MaxCount}", playerId, maxCount);
        if(!await _dbContext.PlayerAccounts.AnyAsync()) {
            throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        }

        if(maxCount > 0) {
            return await _dbContext.PlayerAccounts.Include(i => i.Profile)
                .Where(i => i.Id != playerId && (PlayerAccountStatus)i.Status < PlayerAccountStatus.Banned)
                .OrderBy(x => EF.Functions.Random()).Take(maxCount)
                .Select(i => MaskedPlayerAccountFromPlayerAccount(i)).ToListAsync();
        }
        return null;
    }

    public static MaskedPlayerAccount MaskedPlayerAccountFromPlayerAccount(PlayerAccount playerAccount) => new()
    {
        Code = playerAccount.Code,
        LastLogin = playerAccount.LastLogin,
        Profile = playerAccount.Profile != null ? MaskedPlayerProfileFromPlayerProfile(playerAccount.Profile) : null
    };

    public static FormalPlayerAccount FormalPlayerAccountFromPlayerAccount(PlayerAccount playerAccount) => new()
    {
        Code = playerAccount.Code,
        Status = (PlayerAccountStatus)playerAccount.Status,
        Since = playerAccount.Since,
        LastLogin = playerAccount.LastLogin,
        Profile = playerAccount.Profile != null ? FormalPlayerProfileFromPlayerProfile(playerAccount.Profile) : null
    };

    public static FormalPlayerProfile FormalPlayerProfileFromPlayerProfile(PlayerProfile playerProfile) => new()
    {
        OwnerCode = playerProfile.Owner.Code,
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
