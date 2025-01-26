#nullable disable
using YourGameServer.Interface;

namespace YourGameServer.Models;

public record ServiceTicket
{
    public ulong Id { get; init; }
    public string Name { get; set; }
    public ServiceTicketKind Kind { get; set; }
    public string ProductName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public ulong DetailId { get; set; }
    public ulong IconBlobId { get; set; }
}
