#nullable disable
using System.ComponentModel.DataAnnotations;

namespace YourGameServer.Shared.Models;

public enum ServiceTicketKind
{
    [Display(Name = "Loot Box")]
    LootBox,
    [Display(Name = "Foo Bar")]
    FooBar,
}