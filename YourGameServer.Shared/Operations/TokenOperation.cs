#nullable disable
using Microsoft.EntityFrameworkCore;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;

namespace YourGameServer.Shared.Operations;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public static class TokenOperation
{
    public enum Result
    {
        Succeeded,
        Shortage,
        BadArgument
    }

    public static async Task<Result> AddFreeAsync(GameDbContext dbContext, ulong playerId, ConsumableOrigin origin, int quantity)
    {
        if(quantity <= 0) {
            return Result.BadArgument;
        }
        await dbContext.PlayerOwnedFreeServiceTokens.AddAsync(new() {
            OwnerId = playerId,
            Idx = (await dbContext.PlayerOwnedFreeServiceTokens.Where(x => x.OwnerId == playerId)?.MaxAsync(x => (int?)x.Idx) ?? 0) + 1,
            Origin = origin,
            Quantity = quantity,
            Used = 0,
            Status = ConsumableStatus.Available,
            Since = DateTime.UtcNow,
            LastUsedDate = null
        });
        return Result.Succeeded;
    }

    public static async Task<Result> AddPaidAsync(GameDbContext dbContext, ulong playerId, Store store, int quantity)
    {
        if(quantity <= 0) {
            return Result.BadArgument;
        }
        await dbContext.PlayerOwnedPaidServiceTokens.AddAsync(new() {
            OwnerId = playerId,
            Idx = (await dbContext.PlayerOwnedPaidServiceTokens.Where(x => x.OwnerId == playerId)?.MaxAsync(x => (int?)x.Idx) ?? 0) + 1,
            Store = store,
            Quantity = quantity,
            Used = 0,
            Status = ConsumableStatus.Available,
            Since = DateTime.UtcNow,
            LastUsedDate = null
        });
        return Result.Succeeded;
    }

    public static async Task<Result> ExchangePaidToFreeAsync(GameDbContext dbContext, ulong playerId, Store store, int amount)
    {
        if(amount <= 0) {
            return Result.BadArgument;
        }
        var tokens = dbContext.PlayerOwnedPaidServiceTokens
            .Where(x => x.OwnerId == playerId && x.Status == ConsumableStatus.Available && x.Quantity - x.Used > 0
                && (x.Store == store || x.Store >= Store.Stripe/*External Payment*/));
        if(await tokens.SumAsync(x => x.Quantity - x.Used) < amount) {
            return Result.Shortage;
        }
        while(amount > 0) {
            var item = await tokens.OrderBy(x => x.Since).FirstAsync();
            if(item.Quantity - item.Used > amount) {
                amount = 0;
                item.Used += amount;
            }
            else {
                var delta = item.Quantity - item.Used;
                amount -= delta;
                item.Used = item.Quantity;
                item.Status = ConsumableStatus.Consumed;
            }
            item.LastUsedDate = DateTime.UtcNow;
        }
        return await AddFreeAsync(dbContext, playerId, ConsumableOrigin.Purchase, amount);
    }

    public static async Task<Result> ConsumeAsync(GameDbContext dbContext, ulong playerId, Store store, int amount)
    {
        if(amount <= 0) {
            return Result.BadArgument;
        }
        var paidTokens = dbContext.PlayerOwnedPaidServiceTokens
            .Where(x => x.OwnerId == playerId && x.Status == ConsumableStatus.Available && x.Quantity - x.Used > 0
                && (x.Store == store || x.Store >= Store.Stripe/*External Payment*/));
        var freeTokens = dbContext.PlayerOwnedFreeServiceTokens
            .Where(x => x.OwnerId == playerId && x.Status == ConsumableStatus.Available && x.Quantity - x.Used > 0);
        var paidCount = await paidTokens.SumAsync(x => x.Quantity - x.Used);
        if(await freeTokens.SumAsync(x => x.Quantity - x.Used) + paidCount < amount) {
            return Result.Shortage;
        }
        while(amount > 0) {
            if(paidCount > 0) {
                var item = await paidTokens.OrderBy(x => x.Since).ThenBy(x => x.Store >= Store.Stripe).FirstAsync();
                if(item.Quantity - item.Used > amount) {
                    amount = 0;
                    item.Used += amount;
                }
                else {
                    var delta = item.Quantity - item.Used;
                    amount -= delta;
                    paidCount -= delta;
                    item.Used = item.Quantity;
                    item.Status = ConsumableStatus.Consumed;
                }
                item.LastUsedDate = DateTime.UtcNow;
            }
            else {
                var item = await freeTokens.OrderBy(x => x.Since).FirstAsync();
                if(item.Quantity - item.Used > amount) {
                    amount = 0;
                    item.Used += amount;
                }
                else {
                    var delta = item.Quantity - item.Used;
                    amount -= delta;
                    item.Used = item.Quantity;
                    item.Status = ConsumableStatus.Consumed;
                }
                item.LastUsedDate = DateTime.UtcNow;
            }
        }
        return Result.Succeeded;
    }
}
