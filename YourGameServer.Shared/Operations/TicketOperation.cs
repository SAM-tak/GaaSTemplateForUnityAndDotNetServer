#nullable disable
using Microsoft.EntityFrameworkCore;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;

namespace YourGameServer.Shared.Operations;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public static class TicketOperation
{
    public enum Result
    {
        Succeeded,
        Shortage,
        BadArgument
    }

    public static async Task<Result> AddAsync(GameDbContext dbContext, ulong playerId, string ticketId, int quantity)
    {
        if(quantity <= 0) {
            return Result.BadArgument;
        }
        await dbContext.PlayerOwnedServiceTickets.AddAsync(new() {
            OwnerId = playerId,
            Idx = (await dbContext.PlayerOwnedServiceTickets.Where(x => x.OwnerId == playerId)?.MaxAsync(x => (int?)x.Idx) ?? 0) + 1,
            ServiceTicketId = ticketId,
            Quantity = quantity,
            Used = 0,
            Status = ConsumableStatus.Available,
            Since = DateTime.UtcNow,
            LastUsedDate = null
        });
        return Result.Succeeded;
    }

    public static async Task<Result> ConsumeAsync(GameDbContext dbContext, ulong playerId, string ticketId, int amount)
    {
        if(amount <= 0) {
            return Result.BadArgument;
        }
        var tickets = dbContext.PlayerOwnedServiceTickets
            .Where(x => x.OwnerId == playerId && x.ServiceTicketId == ticketId && x.Status == ConsumableStatus.Available && x.Quantity - x.Used > 0);
        var totalCount = await tickets.SumAsync(x => x.Quantity - x.Used);
        if(totalCount < amount) {
            return Result.Shortage;
        }
        while(amount > 0) {
            var item = await tickets.OrderBy(x => x.Since).FirstAsync();
            if(item.Quantity - item.Used > amount) {
                item.Used += amount;
                amount = 0;
            }
            else {
                amount -= item.Quantity - item.Used;
                item.Used = item.Quantity;
                item.Status = ConsumableStatus.Consumed;
            }
            item.LastUsedDate = DateTime.UtcNow;
        }
        return Result.Succeeded;
    }
}
