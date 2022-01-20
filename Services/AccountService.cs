#nullable disable
using System;
using System.ComponentModel;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YourGameServer.Data;
using YourGameServer.Interface;
using YourGameServer.Models;

namespace YourGameServer.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class AccountService : ServiceBase<IAccountService>, IAccountService
{
    readonly GameDbContext _context;
    readonly JwtAuthorizer _jwt;
    readonly IHttpContextAccessor _httpContextAccessor;

    public AccountService(GameDbContext context, JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwt = jwt;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// LogIn
    /// </summary>
    /// <param name="param">request parameter</param>
    /// <returns>response</returns>
    public async UnaryResult<LogInRequestResult> LogIn(LogInRequest param)
    {
        Console.WriteLine("Login");
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == param.Id);
        if(playerAccount != null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.DeviceType == param.DeviceType && i.DeviceId == param.DeviceId);
            if(playerDevice != null) {
                var utcNow = DateTime.UtcNow;
                if(!string.IsNullOrEmpty(param.NewDeviceId) && param.NewDeviceId != param.DeviceId) {
                    playerDevice = new PlayerDevice {
                        OwnerId = playerAccount.Id,
                        DeviceType = param.DeviceType,
                        DeviceId = param.NewDeviceId,
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
                    DeviceId = playerDevice.Id,
                    Token = _jwt.CreateToken(playerAccount.Id, playerDevice.Id)
                };
            }
        }
        return null;
    }

    /// <summary>
    /// Request new token
    /// </summary>
    /// <returns>new token</returns>
    [FromTypeFilter(typeof(RpcAuthAttribute))]
    public async UnaryResult<RenewTokenRequestResult> RenewToken()
    {
        await Task.CompletedTask;
        Console.WriteLine("RenewToken");
        ulong playerId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["playerid"]);
        ulong deviceId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["deviceid"]);
        return new RenewTokenRequestResult { Token = _jwt.CreateToken(playerId, deviceId) };
    }

    /// <summary>
    /// Sign Up (Create New Account)
    /// </summary>
    /// <param name="signup"></param>
    /// <returns></returns>
    public async UnaryResult<SignInRequestResult> SignUp(SignInRequest signup)
    {
        Console.WriteLine("SignUp");
        if(!string.IsNullOrWhiteSpace(signup.DeviceId)) {
            var playerAccount = await CreateAccountAsync(_context, signup);
            return new SignInRequestResult {
                Id = playerAccount.Id,
                DeviceId = playerAccount.CurrentDeviceId,
                Token = _jwt.CreateToken(playerAccount.Id, playerAccount.CurrentDeviceId)
            };
        }
        return null;
    }

    public static async Task<PlayerAccount> CreateAccountAsync(GameDbContext context, SignInRequest accountCreationModel)
    {
        var code = await LUID.NewLUIDStringAsync(async (i) => !await context.PlayerAccounts.AnyAsync(x => x.Code == i));
        var curDateTime = DateTime.UtcNow;
        var playerAccount = new PlayerAccount {
            Code = code,
            DeviceList = new() {
                new () {
                    DeviceType = accountCreationModel.DeviceType,
                    DeviceId = accountCreationModel.DeviceId,
                    Since = curDateTime,
                    LastUsed = curDateTime,
                }
            },
            Since = curDateTime,
            LastLogin = curDateTime,
            Profile = new() {
                LastUpdate = curDateTime,
            }
        };
        await context.AddAsync(playerAccount);
        await context.SaveChangesAsync();
        playerAccount.CurrentDeviceId = playerAccount.DeviceList.First().Id;
        await context.SaveChangesAsync();
        return playerAccount;
    }
}
