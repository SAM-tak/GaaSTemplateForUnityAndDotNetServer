#nullable disable
using MagicOnion;

namespace YourGameServer.Interface
{
    // Defines .NET interface as a Server/Client IDL.
    // The interface is shared between server and client.
    public interface IAccountService : IService<IAccountService>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        UnaryResult<LogInRequestResult> LogIn(LogInRequest param);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signup"></param>
        /// <returns></returns>
        UnaryResult<SignInRequestResult> SignUp(SignInRequest signup);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        UnaryResult<RenewTokenRequestResult> RenewToken();
    }
}
