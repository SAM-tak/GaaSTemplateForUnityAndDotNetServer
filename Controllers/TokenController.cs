using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Models;
using YourGameServer.Data;
using YourGameServer.Interface;

namespace YourGameServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogInController : ControllerBase
{
    readonly GameDbContext _context;
    readonly JwtTokenGenarator _jwt;
    public LogInController(GameDbContext context, JwtTokenGenarator jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    /// <summary>
    /// Log in (get new token)
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] TokenRequest login)
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
                playerAccount.CurrentDeviceId = playerDevice.Id;
                _context.Entry(playerAccount).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new TokenRequestResult {
                    DeviceId = playerDevice.Id,
                    Token = _jwt.CreateToken(playerAccount.Id, playerDevice.Id)
                });
            }
        }
        return BadRequest();
    }
}
