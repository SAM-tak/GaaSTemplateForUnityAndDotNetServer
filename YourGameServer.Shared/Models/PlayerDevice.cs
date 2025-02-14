#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Shared.Models;

[PrimaryKey(nameof(OwnerId), nameof(Idx))]
public record PlayerDevice
{
    [Display(Name = "Owner Id")]
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId"), JsonIgnore]
    public PlayerAccount Owner { get; init; }
    public int Idx { get; set; }
    [Display(Name = "Device Type")]
    public DeviceType DeviceType { get; set; }
    [Display(Name = "Device ID")]
    public string DeviceId { get; set; }
    [Display(Name = "Since")]
    public DateTime? Since { get; set; }
    [Display(Name = "Last Used Date & Time")]
    public DateTime? LastUsed { get; set; }

    public override int GetHashCode() => HashCode.Combine(OwnerId, Idx, DeviceType, DeviceId, Since, LastUsed);

    public override string ToString() => $"{nameof(OwnerId)}={OwnerId}, {nameof(Idx)}={Idx}, {nameof(DeviceType)}={DeviceType}, {nameof(DeviceId)}={DeviceId}, {nameof(Since)}={Since}, {nameof(LastUsed)}={LastUsed}";
}
