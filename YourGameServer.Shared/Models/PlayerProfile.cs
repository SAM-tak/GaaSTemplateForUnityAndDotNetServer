#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YourGameServer.Shared.Models;

public record PlayerProfile
{
    public ulong Id { get; init; }
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId"), JsonIgnore]
    public PlayerAccount Owner { get; init; }
    public DateTime? LastUpdate { get; set; }
    public string Name { get; set; }
    public string Motto { get; set; }
    public ulong IconBlobId { get; set; }

    public override int GetHashCode() => HashCode.Combine(Id, OwnerId, Name, Motto, IconBlobId);

    public override string ToString() => $"{nameof(Id)}={Id}, {nameof(OwnerId)}={OwnerId}, {nameof(Name)}={Name}, {nameof(Motto)}={Motto}, {nameof(IconBlobId)}={IconBlobId}";
}
