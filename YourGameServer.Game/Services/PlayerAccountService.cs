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
[FromTypeFilter(typeof(VerifyToken))]
public class PlayerAccountService(GameDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger)
    : ServiceBase<IPlayerAccountService>, IPlayerAccountService
{
    readonly GameDbContext _dbContext = dbContext;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<AccountService> _logger = logger;

    public async UnaryResult<FormalPlayerAccount> GetPlayerAccount()
        => FormalPlayerAccountFromPlayerAccount(await _dbContext.PlayerAccounts.FindAsync(_httpContextAccessor.GetPlayerId())
        ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found."));

    public async UnaryResult<IEnumerable<MaskedPlayerAccount>> GetPlayerAccounts(string[] codes)
    {
        var playerId = _httpContextAccessor.GetPlayerId();
        _logger.LogInformation("{PlayerId}|GetPlayerAccounts {Request}", playerId, codes);
        if(!await _dbContext.PlayerAccounts.AnyAsync()) {
            throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        }
        if(codes == null) {
            throw new ReturnStatusException(StatusCode.InvalidArgument, "codes are null.");
        }

        Dictionary<ulong, ushort> idsecrets;
        try {
            idsecrets = codes.Select(IDCoder.DecodeFromPlayerCode).ToDictionary(x => x.Item1, x => x.Item2);
        }
        catch(Exception e) {
            throw new ReturnStatusException(StatusCode.InvalidArgument, $"invalid player code included. {e}");
        }

        if(idsecrets == null || idsecrets.Count == 0) {
            return null;
        }

        // when executing service method 'GetPlayerAccounts'.System.InvalidOperationException:
        // The LINQ expression
        // 'DbSet<PlayerAccount>().Where(p => __idsecrets_0.ContainsKey(p.Id) && (int)__idsecrets_0.get_Item(p.Id) == (int)p.Secret && (int)(PlayerAccountStatus)p.Status < 2)'
        // could not be translated. 
        return (await _dbContext.PlayerAccounts.Include(i => i.Profile)
            .Where(i => idsecrets.Keys.Contains(i.Id) && (PlayerAccountStatus)i.Status < PlayerAccountStatus.Banned)
            .ToListAsync())
            .Where(x => x.Secret == idsecrets[x.Id])
            .Select(MaskedPlayerAccountFromPlayerAccount);
    }

    public async UnaryResult<IEnumerable<MaskedPlayerAccount>> FindPlayerAccounts(int maxCount)
    {
        var playerId = _httpContextAccessor.GetPlayerId();
        _logger.LogInformation("{PlayerId}|FindPlayerAccounts {MaxCount}", playerId, maxCount);
        if(!await _dbContext.PlayerAccounts.AnyAsync()) {
            throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        }
        if(maxCount == 0) {
            return null;
        }
        return await _dbContext.PlayerAccounts.Include(i => i.Profile)
            .Where(i => i.Id != playerId && (PlayerAccountStatus)i.Status < PlayerAccountStatus.Banned)
            .OrderBy(x => EF.Functions.Random()).Take(maxCount)
            .Select(i => MaskedPlayerAccountFromPlayerAccount(i)).ToListAsync();
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
