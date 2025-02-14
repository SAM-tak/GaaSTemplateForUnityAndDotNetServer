#nullable disable
namespace YourGameServer.Shared.Models;

public record ServiceToken
{
    public string Id { get; set; }
    public string ProductName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public ulong IconBlobId { get; set; }
}
