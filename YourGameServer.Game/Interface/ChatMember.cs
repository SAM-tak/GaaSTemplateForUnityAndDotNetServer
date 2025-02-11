#nullable disable // Server needs this
using System; // Unity needs this
using MessagePack;

namespace YourGameServer.Game.Interface // Unity cannot use file-scope namespace yet
{
    /// <summary>
    /// Room participation information
    /// </summary>
    [MessagePackObject]
    public struct ChatMember
    {
        [Key(0)]
        public string PlayerName { get; set; }
        [Key(1)]
        public string PlayerCode { get; set; }
    }
}
