using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Models;
using YourGameServer.Data;

namespace YourGameServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SignInController : ControllerBase
{
    readonly GameDbContext _context;
    readonly JwtTokenGenarator _jwt;

    public SignInController(GameDbContext context, JwtTokenGenarator jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<AccountCreationResult>> SignUp([FromBody] AccountCreationRequest signin)
    {
        if(!string.IsNullOrWhiteSpace(signin.DeviceId)) {
            var playerAccount = await CreateAccountAsync(_context, signin);
            await _context.AddAsync(playerAccount);
            playerAccount.CurrentDeviceId = playerAccount.DeviceList[0].Id;
            await _context.SaveChangesAsync();
            return Ok(new AccountCreationResult {
                Id = playerAccount.Id,
                DeviceId = playerAccount.CurrentDeviceId,
                Token = _jwt.CreateToken(playerAccount.Id, playerAccount.CurrentDeviceId)
            });
        }
        return BadRequest();
    }

    public static async Task<PlayerAccount> CreateAccountAsync(GameDbContext context, AccountCreationRequest accountCreationModel)
    {
        var code = await LUID.NewLUIDStringAsync(async (i) => !await context.PlayerAccounts.AnyAsync(x => x.Code == i));
        var curDateTime = DateTime.UtcNow;
        return new PlayerAccount {
            Code = code,
            DeviceList = new() {
                new() {
                    DeviceType = accountCreationModel.DeviceType,
                    DeviceId = accountCreationModel.DeviceId,
                    Since = curDateTime,
                    LastUsed = curDateTime,
                }
            },
            Since = curDateTime,
            LastLogin = curDateTime,
            Profile = new()
        };
    }
}
