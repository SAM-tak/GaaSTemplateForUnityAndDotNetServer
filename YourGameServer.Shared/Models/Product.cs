namespace YourGameServer.Shared.Models;

public record Product
{
    public string Id { get; set; } = string.Empty;
    public Store Store { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ulong IconBlobId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
