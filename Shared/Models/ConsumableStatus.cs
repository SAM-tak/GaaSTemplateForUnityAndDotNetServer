#nullable disable
using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;
public enum ConsumableStatus
{
    [Display(Name = "Available")]
    Available,
    [Display(Name = "Consumed")]
    Consumed,
    [Display(Name = "Invalid")]
    Invalid,
    [Display(Name = "Expired")]
    Expired,
}
