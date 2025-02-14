#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace YourGameServer.Shared.Models;

public record PlayerAccount
{
    [Display(Name = "ID")]
    public ulong Id { get; init; }
    [JsonIgnore]
    public List<PlayerDevice> DeviceList { get; init; }
    [Display(Name = "Current DeviceId")]
    public int CurrentDeviceIdx { get; set; }
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

    public override int GetHashCode() => (Id, CurrentDeviceIdx, Status, Since, LastLogin, InactivateDate, BanDate, ExpireDate).GetHashCode();

    public string LoginKey => IDCoder.EncodeForLoginKey(Id);

    public string Code => IDCoder.Encode(Id);
}
