using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum ServiceTicketKind
{
    [Display(Name = "Loot Box")]
    LootBox,
    [Display(Name = "Character")]
    Character,
    [Display(Name = "Character Fragment")]
    CharacterFragment,
    [Display(Name = "Gold")]
    Gold,
}