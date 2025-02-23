using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Shared.Models;

[PrimaryKey(nameof(TransactionId), nameof(Store))]
public record Billing
{
    [Display(Name = "Transaction Id (Store Order ID)")]
    public string TransactionId { get; set; } = string.Empty;
    public Store Store { get; set; }
    [Display(Name = "Owner Id")]
    public ulong PlayerId { get; set; }
    [ForeignKey("PlayerId"), JsonIgnore]
    public PlayerAccount? Owner { get; init; }
    public string ProductId { get; set; } = string.Empty;
    [ForeignKey("ProductId"), JsonIgnore]
    public Product? Product { get; init; }
    public string Payload { get; set; } = string.Empty;
    public PurchasingStatus Status { get; set; }
    public DateTime Since { get; set; }
    public DateTime? LastUpdate { get; set; }
    public DateTime? ConsumedAt { get; set; }

    public override int GetHashCode() => (TransactionId, Store, PlayerId, ProductId, Status, Since, LastUpdate).GetHashCode();

    public override string ToString() => $"{nameof(TransactionId)}={TransactionId}, {nameof(Store)}={Store}, {nameof(PlayerId)}={PlayerId}, {nameof(ProductId)}={ProductId}, {nameof(Status)}={Status}, {nameof(Since)}={Since}, {nameof(LastUpdate)}={LastUpdate}, {nameof(ConsumedAt)}={ConsumedAt}";
}
