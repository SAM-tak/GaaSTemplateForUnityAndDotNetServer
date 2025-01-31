#nullable disable
using System; // Unity needs this
using MessagePack;
using YourGameServer.Shared.Models;

namespace YourGameServer.Game.Interface // Unity cannot accpect 'namespace YourProjectName.Interface;' yet
{
    public enum PlayerAccountStatus
    {
        Active,
        Inactive,
        Banned,
        Expired,
    }

    [MessagePackObject]
    public record MaskedPlayerAccount
    {
        [Key(0)]
        public ulong Id { get; init; }
        [Key(1)]
        public DateTime? LastLogin { get; init; }
        [Key(2)]
        public MaskedPlayerProfile Profile { get; init; }

        public static MaskedPlayerAccount FromPlayerAccount(PlayerAccount playerAccount) => new() {
            Id = playerAccount.Id,
            LastLogin = playerAccount.LastLogin,
            Profile = playerAccount.Profile != null ? MaskedPlayerProfile.FromPlayerProfile(playerAccount.Profile) : null
        };
    }

    [MessagePackObject]
    public record FormalPlayerAccount
    {
        [Key(0)]
        public ulong Id { get; init; }
        [Key(1)]
        public string Code { get; init; }
        [Key(2)]
        public PlayerAccountStatus Status { get; init; }
        [Key(3)]
        public DateTime? Since { get; init; }
        [Key(4)]
        public DateTime? LastLogin { get; init; }
        [Key(5)]
        public FormalPlayerProfile Profile { get; init; }

        public static FormalPlayerAccount FromPlayerAccount(PlayerAccount playerAccount) => new() {
            Id = playerAccount.Id,
            Code = playerAccount.Code,
            Status = (PlayerAccountStatus)playerAccount.Status,
            Since = playerAccount.Since,
            LastLogin = playerAccount.LastLogin,
            Profile = playerAccount.Profile != null ? FormalPlayerProfile.FromPlayerProfile(playerAccount.Profile) : null
        };
    }

    [MessagePackObject]
    public record FormalPlayerProfile
    {
        [Key(0)]
        public ulong Id { get; init; }
        [Key(1)]
        public ulong OwnerId { get; set; }
        [Key(2)]
        public DateTime? LastUpdate { get; set; }
        [Key(3)]
        public string Name { get; set; }
        [Key(4)]
        public string Motto { get; set; }
        [Key(5)]
        public ulong IconBlobId { get; set; }

        public static FormalPlayerProfile FromPlayerProfile(PlayerProfile playerProfile) => new() {
            Id = playerProfile.Id,
            OwnerId = playerProfile.OwnerId,
            LastUpdate = playerProfile.LastUpdate,
            Name = playerProfile.Name,
            Motto = playerProfile.Motto,
            IconBlobId = playerProfile.IconBlobId
        };
    }

    [MessagePackObject]
    public record MaskedPlayerProfile
    {
        [Key(0)]
        public string Name { get; init; }
        [Key(1)]
        public string Motto { get; init; }
        [Key(2)]
        public ulong IconBlobId { get; init; }

        public static MaskedPlayerProfile FromPlayerProfile(PlayerProfile playerProfile) => new() {
            Name = playerProfile.Name,
            Motto = playerProfile.Motto,
            IconBlobId = playerProfile.IconBlobId
        };
    }
}
