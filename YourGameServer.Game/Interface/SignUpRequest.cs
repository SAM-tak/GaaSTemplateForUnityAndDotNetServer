#nullable disable
using System; // Unity needs this
using MessagePack;

namespace YourGameServer.Interface // Unity cannot accpect 'namespace YourProjectName.Interface;' yet
{
    [MessagePackObject]
    public record SignUpRequest
    {
        [Key(0)]
        public DeviceType DeviceType { get; init; }
        [Key(1)]
        public string DeviceId { get; init; } // Unity's SystemInfo.deviceUniqueIdentifier
        // 何らかのOpenIDを色々受け付ける
    }

    [MessagePackObject]
    public record SignUpRequestResult
    {
        [Key(0)]
        public ulong Id { get; init; }
        [Key(1)]
        public string Code { get; init; }
        [Key(2)]
        public string Token { get; init; }
        [Key(3)]
        public DateTime Period { get; init; }
    }
}
