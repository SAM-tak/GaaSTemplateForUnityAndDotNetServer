using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using YourGameServer.Models;
using YourGameServer.Data;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    readonly GameDbContext _context;
    public TokenController(GameDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<string>> SignUp([FromBody] AccountCreationModel signUp)
    {
        if(!string.IsNullOrWhiteSpace(signUp.DeviceId)) {
            var playerAccount = await PlayerAccountsController.CreateAsync(_context, signUp);
            await _context.SaveChangesAsync();
            return Ok(playerAccount.PlayerId);
        }
        return BadRequest();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] TokenRequestModel login)
    {
        var playerAccount = await _context.PlayerAccounts.FirstOrDefaultAsync(i => i.PlayerId == login.PlayerId);
        if(playerAccount != null && playerAccount.DeviceList.Any(i => i.DeviceId == login.DeviceId)) {
            return Ok(BuildToken(playerAccount.Id.ToString(), playerAccount.Secret));
        }
        return BadRequest();
    }

    //token生成関数
    private static string BuildToken(string id, byte[] secret)
    {
        var key = new SymmetricSecurityKey(secret);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "YourGameServer",
            audience: id,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
