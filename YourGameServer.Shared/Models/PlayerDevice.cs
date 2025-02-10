#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YourGameServer.Shared.Models;

public record PlayerDevice
{
    [Display(Name = "ID")]
    public ulong Id { get; init; }
    [Display(Name = "Owner Id")]
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId"), JsonIgnore]
    public PlayerAccount Owner { get; init; }
    [Display(Name = "Device Type")]
    public DeviceType DeviceType { get; set; }
    [Display(Name = "Device ID")]
    public string DeviceId { get; set; }
    [Display(Name = "Since")]
    public DateTime? Since { get; set; }
    [Display(Name = "Last Used Date & Time")]
    public DateTime? LastUsed { get; set; }

    public override int GetHashCode() => HashCode.Combine(Id, OwnerId, DeviceType, DeviceId, Since, LastUsed);

    public override string ToString() => $"{nameof(Id)}={Id}, {nameof(OwnerId)}={OwnerId}, {nameof(DeviceType)}={DeviceType}, {nameof(DeviceId)}={DeviceId}, {nameof(Since)}={Since}, {nameof(LastUsed)}={LastUsed}";
}
