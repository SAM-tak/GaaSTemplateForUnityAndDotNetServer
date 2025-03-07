#nullable disable // Server needs this
using System.Collections.Generic; // Unity needs this
using MagicOnion;

namespace YourGameServer.Game.Interface // Unity cannot use file-scope namespace yet
{
    // Defines .NET interface as a Server/Client IDL.
    // The interface is shared between server and client.
    public interface IServiceTokenService : IService<IServiceTokenService>
    {
        UnaryResult<OwnedServiceTokens> GetOwnedTokens();
    }
}
