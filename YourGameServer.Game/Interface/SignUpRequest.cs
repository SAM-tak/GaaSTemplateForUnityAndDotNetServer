#nullable disable // Server needs this
using System; // Unity needs this
using MessagePack;

namespace YourGameServer.Game.Interface // Unity cannot use file-scope namespace yet
{
    [MessagePackObject]
    public record SignUpRequest
    {
        [Key(0)]
        public DeviceType DeviceType { get; init; }
        [Key(1)]
        public string DeviceIdentifier { get; init; } // Unity's SystemInfo.deviceUniqueIdentifier
        [Key(2)]
        public Store OfficialStore { get; init; }
        // 何らかのOpenIDを色々受け付ける
    }

    [MessagePackObject]
    public record SignUpRequestResult
    {
        [Key(0)]
        public string Token { get; init; }
        [Key(1)]
        public DateTime Period { get; init; }
        [Key(2)]
        public string LoginKey { get; init; }
        [Key(3)]
        public string Code { get; init; }
    }
}
