#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YourGameServer.Shared.Models;

public record PlayerOwnedServiceTicket
{
    public ulong Id { get; init; }
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId"), JsonIgnore]
    public PlayerAccount Owner { get; init; }
    public ulong ServiceTicketId { get; set; }
    [ForeignKey("ServiceTicketId"), JsonIgnore]
    public ServiceTicket ServiceTicket { get; init; }
    public ConsumableOrigin Origin { get; set; }
    public ConsumableStatus Status { get; set; }
    public int Quantity { get; set; }
    public int Used { get; set; }
    public DateTime? Period { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public DateTime? InvalidateDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public override int GetHashCode() => (Id, OwnerId, ServiceTicketId, Origin, Status, Quantity, Used, Period, LastUsedDate, InvalidateDate, ExpireDate).GetHashCode();

    public override string ToString() => $"{nameof(Id)}={Id}, {nameof(OwnerId)}={OwnerId}, {nameof(ServiceTicketId)}={ServiceTicketId}, {nameof(Origin)}={Origin}, {nameof(Status)}={Status}, {nameof(Quantity)}={Quantity}, {nameof(Used)}={Used}, {nameof(Period)}={Period}, {nameof(LastUsedDate)}={LastUsedDate}, {nameof(InvalidateDate)}={InvalidateDate}, {nameof(ExpireDate)}={ExpireDate}";
}
