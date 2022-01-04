using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using YourGameServer.Models;
using YourGameServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

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
            await _context.SaveChangesAsync();
            return Ok(new AccountCreationResult {
                PlayerId = playerAccount.PlayerId,
                Token = _jwt.CreateToken(playerAccount.Id)
            });
        }
        return BadRequest();
    }

    public static async Task<PlayerAccount> CreateAccountAsync(GameDbContext context, AccountCreationRequest accountCreationModel)
    {
        var playerId = await LUID.NewLUIDStringAsync(async (i) => !await context.PlayerAccounts.AnyAsync(x => x.PlayerId == i));
        var curDateTime = DateTime.UtcNow;
        return new PlayerAccount {
            PlayerId = playerId,
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
