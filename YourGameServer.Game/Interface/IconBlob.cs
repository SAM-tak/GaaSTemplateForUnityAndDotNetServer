#nullable disable
using MessagePack;

namespace YourGameServer.Game.Interface // Unity cannot accpect 'namespace YourProjectName.Interface;' yet
{
    [MessagePackObject]
    public record IconBlob
    {
        [Key(0)]
        public string address;
    }
}
