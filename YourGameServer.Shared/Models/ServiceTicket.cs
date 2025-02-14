#nullable disable
namespace YourGameServer.Shared.Models;

public record ServiceTicket
{
    public string Id { get; set; }
    public ServiceTicketKind Kind { get; set; }
    public string ProductName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public ulong DetailId { get; set; }
    public ulong IconBlobId { get; set; }
}
