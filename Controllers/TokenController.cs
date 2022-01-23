using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Models;
using YourGameServer.Data;
using YourGameServer.Interface;

namespace YourGameServer.Controllers;

[Route("api/[controller]")]
[ApiAuth]
[ApiController]
public class LogInController : ControllerBase
{
    readonly GameDbContext _context;
    readonly JwtAuthorizer _jwt;
    readonly IHttpContextAccessor _httpContextAccessor;

    public LogInController(GameDbContext context, JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwt = jwt;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Log in
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<LogInRequestResult>> Login([FromBody] LogInRequest login)
    {
        Console.WriteLine("Login");
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == login.Id);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.DeviceType == login.DeviceType && i.DeviceId == login.DeviceId);
            if(playerDevice is not null) {
                var utcNow = DateTime.UtcNow;
                if(playerAccount.CurrentDeviceId != playerDevice.Id && playerDevice.LastUsed.HasValue && utcNow > _jwt.ExpireDate(playerDevice.LastUsed.Value)) {
                    // It will deny that last token not expired yet and login with other device.
                    return Conflict();
                }
                if(!string.IsNullOrEmpty(login.NewDeviceId) && login.NewDeviceId != login.DeviceId) {
                    playerDevice = new PlayerDevice {
                        OwnerId = playerAccount.Id,
                        DeviceType = login.DeviceType,
                        DeviceId = login.NewDeviceId,
                        Since = utcNow,
                        LastUsed = utcNow,
                    };
                    await _context.AddAsync(playerDevice);
                    await _context.SaveChangesAsync();
                }
                playerDevice.LastUsed = playerAccount.LastLogin = utcNow;
                playerAccount.CurrentDeviceId = playerDevice.Id;
                await _context.SaveChangesAsync();
                return new LogInRequestResult {
                    Token = _jwt.CreateToken(playerAccount.Id, playerDevice.Id, out var period),
                    Period = period
                };
            }
        }
        return BadRequest();
    }

    /// <summary>
    /// Request new token
    /// </summary>
    /// <returns>new token</returns>
    [HttpPost]
    public async Task<ActionResult<RenewTokenRequestResult>> RenewToken()
    {
        Console.WriteLine("RenewToken");
        ulong playerId = ulong.Parse(_httpContextAccessor.HttpContext?.Request?.Headers["playerid"]!);
        ulong deviceId = ulong.Parse(_httpContextAccessor.HttpContext?.Request?.Headers["deviceid"]!);
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Id == deviceId);
            if(playerDevice is not null) {
                if(playerAccount.CurrentDeviceId != deviceId) {
                    return StatusCode(412);
                }
                playerDevice.LastUsed = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new RenewTokenRequestResult {
                    Token = _jwt.CreateToken(playerId, deviceId, out var period),
                    Period = period
                };
            }
        }
        return NotFound();
    }

    /// <summary>
    /// Log out
    /// </summary>
    /// <returns>None</returns>
    [HttpPost]
    public async Task<ActionResult> LogOut()
    {
        Console.WriteLine("LogOut");
        ulong playerId = ulong.Parse(_httpContextAccessor.HttpContext?.Request.Headers["playerid"]!);
        ulong deviceId = ulong.Parse(_httpContextAccessor.HttpContext?.Request.Headers["deviceid"]!);
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Id == deviceId);
            if(playerDevice is not null) {
                if(playerAccount.CurrentDeviceId != deviceId) {
                    return StatusCode(412);
                }
                playerAccount.CurrentDeviceId = 0;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
        return NotFound();
    }
}
