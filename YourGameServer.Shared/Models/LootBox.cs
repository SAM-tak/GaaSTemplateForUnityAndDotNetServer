#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace YourGameServer.Shared.Models;

[Table("LootBoxes")]
public record LootBox
{
    public string Id { get; set; }
    public string ProductName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string IconAddress { get; set; }
    public string BannerAddress { get; set; }
}
