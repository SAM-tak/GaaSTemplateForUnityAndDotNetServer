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
public class TokenController : ControllerBase
{
    readonly GameDbContext _context;
    readonly JwtTokenGenarator _jwt;
    public TokenController(GameDbContext context, JwtTokenGenarator jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] TokenRequest login)
    {
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.PlayerId == login.PlayerId);
        if(playerAccount != null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.DeviceId == login.DeviceId);
            if(playerDevice != null) {
                playerAccount.LastLogin = playerDevice.LastUsed = DateTime.UtcNow;
                _context.Entry(playerAccount).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(_jwt.CreateToken(playerAccount.Id));
            }
        }
        return BadRequest();
    }
}
