using System.ComponentModel.DataAnnotations.Schema;
using MessagePack;

namespace YourGameServer.Models // Unity cannot accpect 'namespace YourProjectName.Models;' yet
{
    [NotMapped]
    [MessagePackObject]
    public record TokenRequest
    {
        [Key(0)]
        public string PlayerId { get; init; }
        [Key(1)]
        public string DeviceId { get; init; } // Unity's SystemInfo.deviceUniqueIdentifier
    }
}
