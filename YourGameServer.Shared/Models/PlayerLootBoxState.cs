using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Shared.Models;

[PrimaryKey(nameof(OwnerId), nameof(LootBoxId))]
public record PlayerLootBoxState
{
    [Display(Name = "Owner Id")]
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId"), JsonIgnore]
    public PlayerAccount? Owner { get; init; }
    public string LootBoxId { get; set; } = string.Empty;
    [ForeignKey("LootBoxId"), JsonIgnore]
    public LootBox? LootBox { get; init; }
    public DateTime Since { get; set; }
    public DateTime? LastOpenDate { get; set; }
    public int OpenCount { get; set; }
    public string FeaturedHistory { get; set; } = string.Empty;
    public string LegendaryHistory { get; set; } = string.Empty;
    public string EpicHistory { get; set; } = string.Empty;
    public string RareHistory { get; set; } = string.Empty;
    public string UncommonHistory { get; set; } = string.Empty;
    public string CommonHistory { get; set; } = string.Empty;

    public override int GetHashCode() => (OwnerId, LootBoxId, Since, LastOpenDate, OpenCount).GetHashCode();

    public override string ToString() => $"{nameof(OwnerId)}={OwnerId}, {nameof(LootBoxId)}={LootBoxId}, {nameof(Since)}={Since}, {nameof(LastOpenDate)}={LastOpenDate}, {nameof(OpenCount)}={OpenCount}";
}
