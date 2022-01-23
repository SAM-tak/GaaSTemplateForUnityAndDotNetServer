using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YourGameServer.Interface;
using YourGameServer.Data;

namespace YourGameServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SignUpController : ControllerBase
{
    readonly GameDbContext _context;
    readonly JwtAuthorizer _jwt;

    public SignUpController(GameDbContext context, JwtAuthorizer jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    /// <summary>
    /// Sign Up (Create New Account)
    /// </summary>
    /// <param name="signup"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<SignInRequestResult>> SignUp([FromBody] SignInRequest signup)
    {
        Console.WriteLine("SignUp");
        if(!string.IsNullOrWhiteSpace(signup.DeviceId)) {
            var playerAccount = await Services.AccountService.CreateAccountAsync(_context, signup);
            return new SignInRequestResult {
                Id = playerAccount.Id,
                Token = _jwt.CreateToken(playerAccount.Id, playerAccount.CurrentDeviceId, out var period),
                Period = period
            };
        }
        return BadRequest();
    }
}
