#nullable disable
using System.ComponentModel.DataAnnotations.Schema;
using MessagePack;
using YourGameServer.Models;

namespace YourGameServer.Interface // Unity cannot accpect 'namespace YourProjectName.Models;' yet
{
    [MessagePackObject]
    public record TokenRequest
    {
        [Key(0)]
        public ulong Id { get; init; }
        [Key(1)]
        public DeviceType DeviceType { get; init; }
        [Key(2)]
        public string DeviceId { get; init; } // Unity's SystemInfo.deviceUniqueIdentifier
        [Key(3)]
        public string NewDeviceId { get; init; } // Unity's SystemInfo.deviceUniqueIdentifier
    }

    [MessagePackObject]
    public record TokenRequestResult
    {
        [Key(0)]
        public ulong DeviceId { get; init; } // PlayerDevice Table index
        [Key(1)]
        public string Token { get; init; }
    }
}
