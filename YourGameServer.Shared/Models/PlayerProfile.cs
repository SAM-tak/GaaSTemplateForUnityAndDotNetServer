using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YourGameServer.Shared.Models;

public record PlayerProfile
{
    [Key, Display(Name = "Owner Id")]
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId"), JsonIgnore]
    public PlayerAccount? Owner { get; init; }
    public DateTime? LastUpdate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Motto { get; set; } = string.Empty;
    public ulong IconBlobId { get; set; }

    public override int GetHashCode() => HashCode.Combine(OwnerId, Name, Motto, IconBlobId);

    public override string ToString() => $"{nameof(OwnerId)}={OwnerId}, {nameof(Name)}={Name}, {nameof(Motto)}={Motto}, {nameof(IconBlobId)}={IconBlobId}";
}
