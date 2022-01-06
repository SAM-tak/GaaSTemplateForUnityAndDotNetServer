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
    public async Task<ActionResult<string>> Login([FromBody][FromForm] TokenRequest login)
    {
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == login.Id);
        if(playerAccount != null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.DeviceType == login.DeviceType && i.DeviceId == login.DeviceId);
            if(playerDevice != null) {
                playerAccount.LastLogin = DateTime.UtcNow;
                if(!string.IsNullOrEmpty(login.NewDeviceId) && login.NewDeviceId != login.DeviceId) {
                    playerDevice = new PlayerDevice {
                        DeviceType = login.DeviceType,
                        DeviceId = login.NewDeviceId,
                        Since = playerAccount.LastLogin,
                        LastUsed = playerAccount.LastLogin,
                    };
                    playerAccount.DeviceList.Add(playerDevice);
                }
                playerDevice.LastUsed = playerAccount.LastLogin;
                _context.Entry(playerAccount).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(_jwt.CreateToken(playerAccount.Id));
            }
        }
        return BadRequest();
    }
}
