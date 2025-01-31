#nullable disable
using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public record PlayerAccount
{
    [Display(Name = "ID")]
    public ulong Id { get; init; }
    [Display(Name = "Secret")]
    public ushort Secret { get; set; }
    public List<PlayerDevice> DeviceList { get; init; }
    [Display(Name = "Current DeviceId")]
    public ulong CurrentDeviceId { get; set; }
    [Display(Name = "Kind")]
    public PlayerAccountKind Kind { get; set; }
    [Display(Name = "Status")]
    public PlayerAccountStatus Status { get; set; }
    [Display(Name = "Since")]
    public DateTime? Since { get; set; }
    [Display(Name = "Last Login Time")]
    public DateTime? LastLogin { get; set; }
    [Display(Name = "Inactivate Date")]
    public DateTime? InactivateDate { get; set; }
    [Display(Name = "Ban Date")]
    public DateTime? BanDate { get; set; }
    [Display(Name = "Expire Date")]
    public DateTime? ExpireDate { get; set; }
    [Display(Name = "Profile")]
    public PlayerProfile Profile { get; init; }

    public override int GetHashCode() => (Id, Secret, CurrentDeviceId, Status, Since, LastLogin, InactivateDate, BanDate, ExpireDate).GetHashCode();

    public string Code => IDCoder.Encode(Id, Secret);
}
