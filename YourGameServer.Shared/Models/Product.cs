#nullable disable
namespace YourGameServer.Shared.Models;

public record Product
{
    public string Id { get; set; }
    public Store Store { get; set; }
    public string ProductName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public ulong IconBlobId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
