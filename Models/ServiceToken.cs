#nullable disable

namespace YourGameServer.Models;

public record ServiceToken
{
    public ulong Id { get; init; }
    public string Name { get; set; }
    public string ProductName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public ulong IconBlobId { get; set; }
}
