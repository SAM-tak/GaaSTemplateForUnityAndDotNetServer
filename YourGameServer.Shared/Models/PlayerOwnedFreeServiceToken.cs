#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace YourGameServer.Shared.Models;

public record PlayerOwnedFreeServiceToken
{
    public ulong Id { get; init; }
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public PlayerAccount Owner { get; init; }
    public ConsumableOrigin Origin { get; set; }
    public int Quantity { get; set; }
    public int Used { get; set; }
    public DateTime? Period { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public DateTime? InvalidateDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public override int GetHashCode() => (Id, OwnerId, Origin, Quantity, Used, Period, LastUsedDate, InvalidateDate, ExpireDate).GetHashCode();

    public override string ToString() => $"{nameof(Id)}={Id}, {nameof(OwnerId)}={OwnerId}, {nameof(Origin)}={Origin}, {nameof(Quantity)}={Quantity}, {nameof(Used)}={Used}, {nameof(Period)}={Period}, {nameof(LastUsedDate)}={LastUsedDate}, {nameof(InvalidateDate)}={InvalidateDate}, {nameof(ExpireDate)}={ExpireDate}";
}
