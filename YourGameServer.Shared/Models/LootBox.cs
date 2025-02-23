using System.ComponentModel.DataAnnotations.Schema;

namespace YourGameServer.Shared.Models;

[Table("LootBoxes")]
public record LootBox
{
    public string Id { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconAddress { get; set; } = string.Empty;
    public string BannerAddress { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string FeaturedContent { get; set; } = string.Empty;
    public string LegendaryContent { get; set; } = string.Empty;
    public string EpicContent { get; set; } = string.Empty;
    public string RareContent { get; set; } = string.Empty;
    public string UncommonContent { get; set; } = string.Empty;
    public string CommonContent { get; set; } = string.Empty;
}
