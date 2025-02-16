using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum PlayerAccountStatus
{
    [Display(Name = "Active")]
    Active,
    [Display(Name = "Inactive")]
    Inactive,
    [Display(Name = "Banned")]
    Banned,
    [Display(Name = "Expired")]
    Expired,
}