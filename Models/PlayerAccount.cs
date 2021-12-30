using System; // Unity needs this
using System.ComponentModel;
using MessagePack;

namespace YourGameServer.Models // Unity cannot accpect 'namespace YourProjectName.Models;' yet
{
    public enum PlayerStatus
    {
        [Description("Active")]
        Active,
        [Description("Inactive")]
        Inactive,
        [Description("Banned")]
        Banned,
        [Description("Expired")]
        Expired,
    }

    [MessagePackObject]
    public record PlayerAccount
    {
        [Key(0)]
        public long ID { get; init; }
        [Key(1)]
        public PlayerStatus Status { get; init; }
        [Key(2)]
        public DateTime Since { get; init; }
        [Key(3)]
        public DateTime LastLogin { get; init; }
        [Key(4)]
        public DateTime? InactivateDate { get; init; }
        [Key(5)]
        public DateTime? BanDate { get; init; }
        [Key(6)]
        public DateTime? ExpireDate { get; init; }
    }
}
