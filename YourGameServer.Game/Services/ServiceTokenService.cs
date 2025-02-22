#nullable disable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Game.Extensions;
using YourGameServer.Game.Interface;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;
using Store = YourGameServer.Shared.Models.Store;

namespace YourGameServer.Game.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
[FromTypeFilter(typeof(VerifyTokenAndAccount))]
public class ServiceTokenService(GameDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger)
    : ServiceBase<IServiceTokenService>, IServiceTokenService
{
    readonly GameDbContext _dbContext = dbContext;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<AccountService> _logger = logger;

    public async UnaryResult<OwnedServiceTokens> GetOwnedTokens()
    {
        if(_httpContextAccessor.TryGetPlayerIdAndDeviceIdx(out var playerId, out var deviceIdx)) {
            var device = await _dbContext.PlayerDevices.FindAsync(playerId, deviceIdx)
                ?? throw new ReturnStatusException(StatusCode.FailedPrecondition, "correspond account has no valid device.");
            return await GetOwnedServiceTokensAsync(_dbContext, playerId, device.OfficialStore);
        }
        throw new ReturnStatusException(StatusCode.InvalidArgument, "invalid header.");
    }

    public static async UnaryResult<OwnedServiceTokens> GetOwnedServiceTokensAsync(GameDbContext dbContext, ulong playerId, Store store) => new() {
        Free = await dbContext.PlayerOwnedFreeServiceTokens
                .Where(x => x.OwnerId == playerId && x.Status == ConsumableStatus.Available && x.Quantity - x.Used > 0)
                .SumAsync(x => x.Quantity - x.Used),
        Paid = await dbContext.PlayerOwnedPaidServiceTokens
                .Where(x => x.OwnerId == playerId && x.Status == ConsumableStatus.Available && x.Quantity - x.Used > 0
                    && (x.Store == store || x.Store >= Store.Stripe/*External Payment*/))
                .SumAsync(x => x.Quantity - x.Used)
    };
}
