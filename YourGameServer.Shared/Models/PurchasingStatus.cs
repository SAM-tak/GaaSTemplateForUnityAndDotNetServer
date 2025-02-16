using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum PurchasingStatus
{
    [Display(Name = "Pending")]
    Pending,
    [Display(Name = "Verified")]
    Verified,
    [Display(Name = "Consumed")]
    Consumed,
    [Display(Name = "VerifyFailed")]
    VerifyFailed,
}
