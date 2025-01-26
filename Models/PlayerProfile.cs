#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using YourGameServer.Interface;

namespace YourGameServer.Models;

public record PlayerProfile
{
    public ulong Id { get; init; }
    public ulong OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public PlayerAccount Owner { get; init; }
    public DateTime? LastUpdate { get; set; }
    public string Name { get; set; }
    public string Motto { get; set; }
    public ulong IconBlobId { get; set; }

    public override int GetHashCode() => HashCode.Combine(Id, OwnerId, Name, Motto, IconBlobId);

    public override string ToString() => $"{{{nameof(Id)}={Id}, {nameof(OwnerId)}={OwnerId}, {nameof(Name)}={Name}, {nameof(Motto)}={Motto}, {nameof(IconBlobId)}={IconBlobId}}}";

    public MaskedPlayerProfile MakeMasked() => new() {
        Name = Name,
        Motto = Motto,
        IconBlobId = IconBlobId
    };

    public FormalPlayerProfile MakeFormal() => new() {
        Id = Id,
        OwnerId = OwnerId,
        LastUpdate = LastUpdate,
        Name = Name,
        Motto = Motto,
        IconBlobId = IconBlobId
    };
}
