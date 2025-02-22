namespace YourGameServer.Shared.Models;

public record ServiceTicket
{
    public string Id { get; set; } = string.Empty;
    public ServiceTicketKind Kind { get; set; }
    public Rarity Rarity { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ulong DetailId { get; set; }
    public ulong IconBlobId { get; set; }
}
