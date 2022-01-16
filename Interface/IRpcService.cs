using System;
using MagicOnion;
using YourGameServer.Models;

namespace YourGameServer.Interface
{
    // Defines .NET interface as a Server/Client IDL.
    // The interface is shared between server and client.
    public interface IRpcService : IService<IRpcService>
    {
        // The return type must be `UnaryResult<T>`.
        UnaryResult<TokenRequestResult?> Login(TokenRequest param);
    }
}
