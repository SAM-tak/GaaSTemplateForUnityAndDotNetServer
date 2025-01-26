#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace YourGameServer.Models;

[Table("LootBoxes")]
public record LootBox
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public string ProductName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string IconAddress { get; set; }
    public string BannerAddress { get; set; }
}
