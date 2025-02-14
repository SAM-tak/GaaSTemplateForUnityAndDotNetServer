#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Shared.Models;

[PrimaryKey(nameof(TransactionId), nameof(Store))]
public record PurchaseOrder
{
    [Display(Name = "Transaction Id (Store Order ID)")]
    public string TransactionId { get; set; }
    public Store Store { get; set; }
    [Display(Name = "Owner Id")]
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId"), JsonIgnore]
    public PlayerAccount Owner { get; init; }
    public string ProductId { get; set; }
    [ForeignKey("ProductId"), JsonIgnore]
    public Product Product { get; init; }
    public string Payload { get; set; }
    public PurchasingStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }

    public override int GetHashCode() => (TransactionId, Store, OwnerId, ProductId, Status, CreatedAt, UpdatedAt).GetHashCode();

    public override string ToString() => $"{nameof(TransactionId)}={TransactionId}, {nameof(Store)}={Store}, {nameof(OwnerId)}={OwnerId}, {nameof(ProductId)}={ProductId}, {nameof(Status)}={Status}, {nameof(CreatedAt)}={CreatedAt}, {nameof(UpdatedAt)}={UpdatedAt}, {nameof(ConsumedAt)}={ConsumedAt}";
}
