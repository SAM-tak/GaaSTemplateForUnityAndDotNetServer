using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum Rarity
{
    [Display(Name = "Common")]
    Common,
    // Official stores
    [Display(Name = "Uncommon")]
    Uncommon,
    [Display(Name = "Rare")]
    Rare,
    [Display(Name = "Epic")]
    Epic,
    [Display(Name = "Legendary")]
    Legendary,
}