#nullable disable
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;
using DeviceType = YourGameServer.Shared.Models.DeviceType;

namespace YourGameServer.Shared.Operations;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public static class PlayerAccountOperation
{
    static readonly Random _random = new();

    public static async Task<PlayerAccount> CreateAsync(GameDbContext context, DeviceType deviceType, Store officialStore, string deviceIdentifier)
    {
        var curDateTime = DateTime.UtcNow;
        var playerAccount = new PlayerAccount {
            Secret = (ushort)_random.Next(0, ushort.MaxValue + 1),
            CurrentDeviceIdx = 0,
            DeviceList = [
                new () {
                    Idx = 1,
                    DeviceType = deviceType,
                    DeviceIdentifier = deviceIdentifier,
                    OfficialStore = officialStore,
                    Since = curDateTime,
                    LastUsed = curDateTime,
                }
            ],
            Since = curDateTime,
            LastLogin = curDateTime,
            Profile = new() {
                LastUpdate = curDateTime,
            }
        };
        await context.AddAsync(playerAccount);
        return playerAccount;
    }

    public static async Task<PlayerAccount> RenewSecret(GameDbContext dbContext, ulong playerId)
    {
        var playerAccount = await dbContext.PlayerAccounts.FindAsync(playerId);
        if(playerAccount is not null) {
            playerAccount.Secret = (ushort)_random.Next(0, ushort.MaxValue + 1);
            return playerAccount;
        }
        return null;
    }
}
