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
    public ConsumableStatus Status { get; set; }
    public DateTime? Period { get; set; }
    public DateTime? UsedDate { get; set; }
    public DateTime? InvalidateDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public override int GetHashCode() => HashCode.Combine(Id, OwnerId, Origin, Status, Period, UsedDate, InvalidateDate, ExpireDate);

    public override string ToString() => $"{{{nameof(Id)}={Id}, {nameof(OwnerId)}={OwnerId}, {nameof(Origin)}={Origin}, {nameof(Status)}={Status}, {nameof(Period)}={Period}, {nameof(UsedDate)}={UsedDate}, {nameof(InvalidateDate)}={InvalidateDate}, {nameof(ExpireDate)}={ExpireDate}}}";
}
