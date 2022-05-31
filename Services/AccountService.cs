#nullable disable
using System.Security.Cryptography;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.EntityFrameworkCore;
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
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.DeviceType == param.DeviceType && i.DeviceId == param.DeviceId);
            if(playerDevice is not null) {
                var utcNow = DateTime.UtcNow;
                if (playerAccount.CurrentDeviceId > 0 && playerAccount.CurrentDeviceId != playerDevice.Id && utcNow > _jwt.ExpireDate(playerDevice.LastUsed.Value)) {
                    // It will deny that last token not expired yet and login with other device.
                    throw new ReturnStatusException(StatusCode.AlreadyExists, "already logged in with other device. try later.");
                }
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
                    Token = _jwt.CreateToken(playerAccount.Id, playerDevice.Id, out var period),
                    Period = period
                };
            }
        }
        throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
    }

    /// <summary>
    /// Request new token
    /// </summary>
    /// <returns>new token</returns>
    [FromTypeFilter(typeof(RpcAuthAttribute))]
    public async UnaryResult<RenewTokenRequestResult> RenewToken()
    {
        Console.WriteLine("RenewToken");
        ulong playerId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["playerid"]);
        ulong deviceId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["deviceid"]);
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Id == deviceId);
            if(playerDevice is not null) {
                if(playerAccount.CurrentDeviceId != deviceId) {
                    throw new ReturnStatusException(StatusCode.FailedPrecondition, "You are not logged in with current device.");
                }
                playerDevice.LastUsed = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new RenewTokenRequestResult {
                    Token = _jwt.CreateToken(playerId, deviceId, out var period),
                    Period = period
                };
            }
        }
        throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
    }

    /// <summary>
    /// Log out
    /// </summary>
    /// <returns>None</returns>
    [FromTypeFilter(typeof(RpcAuthAttribute))]
    public async UnaryResult<Nil> LogOut()
    {
        Console.WriteLine("LogOut");
        ulong playerId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["playerid"]);
        ulong deviceId = ulong.Parse(_httpContextAccessor.HttpContext.Request.Headers["deviceid"]);
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Id == deviceId);
            if(playerDevice is not null) {
                if(playerAccount.CurrentDeviceId != deviceId) {
                    throw new ReturnStatusException(StatusCode.FailedPrecondition, "You are not logged in with current device.");
                }
                playerAccount.CurrentDeviceId = 0;
                await _context.SaveChangesAsync();
                return new Nil();
            }
        }
        throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
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
                Token = _jwt.CreateToken(playerAccount.Id, playerAccount.CurrentDeviceId, out var period),
                Period = period
            };
        }
        throw new ReturnStatusException(StatusCode.InvalidArgument, "Device Identifier is invalid.");
    }

    public static async Task<PlayerAccount> CreateAccountAsync(GameDbContext context, SignInRequest accountCreationModel)
    {
        var curDateTime = DateTime.UtcNow;
        var playerAccount = new PlayerAccount {
            Secret = (ushort)RandomNumberGenerator.GetInt32(0x10000),
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
        playerAccount.CensorCodeIfNeeds();
        await context.SaveChangesAsync();
        return playerAccount;
    }
}
