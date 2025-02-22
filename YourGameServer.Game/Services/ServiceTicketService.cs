#nullable disable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Game.Extensions;
using YourGameServer.Game.Interface;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;

namespace YourGameServer.Game.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
[FromTypeFilter(typeof(VerifyTokenAndAccount))]
public class ServiceTicketService(GameDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger)
    : ServiceBase<IServiceTicketService>, IServiceTicketService
{
    readonly GameDbContext _dbContext = dbContext;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<AccountService> _logger = logger;

    public async UnaryResult<IEnumerable<Interface.ServiceTicket>> GetTickets() => await GetTicketsAsync(_dbContext);

    public async UnaryResult<IEnumerable<OwnedServiceTicket>> GetOwnedTickets() => await GetOwnedTicketsAsync(_dbContext, _httpContextAccessor.GetPlayerId());

    public async UnaryResult<IEnumerable<Interface.ServiceTicket>> GetTicketsAsync(GameDbContext dbContext)
        => await dbContext.ServiceTickets.Select(x => ToProtocol(x)).ToListAsync()
            ?? throw new ReturnStatusException(StatusCode.NotFound, "no service ticket defines.");

    public static async UnaryResult<IEnumerable<OwnedServiceTicket>> GetOwnedTicketsAsync(GameDbContext dbContext, ulong playerId)
        => await dbContext.PlayerOwnedServiceTickets.Where(x => x.OwnerId == playerId && x.Status == ConsumableStatus.Available && x.Quantity - x.Used > 0)
            .GroupBy(x => x.ServiceTicketId).Select(x => ToProtocol(x)).ToListAsync()
            ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account has no ticket.");

    public static Interface.ServiceTicket ToProtocol(Shared.Models.ServiceTicket serviceTicket) => new() {
        Id = serviceTicket.Id,
        Kind = (Interface.ServiceTicketKind)serviceTicket.Kind,
        DisplayName = serviceTicket.DisplayName,
        Description = serviceTicket.Description,
        DetailId = serviceTicket.DetailId,
        IconBlobId = serviceTicket.IconBlobId
    };

    public static OwnedServiceTicket ToProtocol(IGrouping<string, PlayerOwnedServiceTicket> serviceTicketGroup) => new() {
        ServiceTicketId = serviceTicketGroup.Key,
        Count = serviceTicketGroup.Sum(x => x.Quantity - x.Used)
    };
}
