#nullable disable
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;
using DeviceType = YourGameServer.Shared.Models.DeviceType;

namespace YourGameServer.Shared.Operations;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public static class AccountOperation
{
    public static async Task<PlayerAccount> CreateAccountAsync(GameDbContext context, DeviceType deviceType, string deviceIdentifier)
    {
        var curDateTime = DateTime.UtcNow;
        var playerAccount = new PlayerAccount {
            CurrentDeviceIdx = 1,
            DeviceList = [
                new () {
                    Idx = 1,
                    DeviceType = deviceType,
                    DeviceIdentifier = deviceIdentifier,
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
}
