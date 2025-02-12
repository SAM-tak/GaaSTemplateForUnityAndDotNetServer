#nullable disable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Game.Extensions;
using YourGameServer.Game.Interface;
using YourGameServer.Shared;
using YourGameServer.Shared.Data;
using YourGameServer.Shared.Models;
using YourGameServer.Shared.Operations;
using DeviceType = YourGameServer.Shared.Models.DeviceType;

namespace YourGameServer.Game.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class AccountService(GameDbContext dbContext, JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor, ILogger<AccountService> logger)
    : ServiceBase<IAccountService>, IAccountService
{
    readonly GameDbContext _dbContext = dbContext;
    readonly JwtAuthorizer _jwt = jwt;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<AccountService> _logger = logger;

    /// <summary>
    /// LogIn
    /// </summary>
    /// <param name="param">request parameter</param>
    /// <returns>response</returns>
    public async UnaryResult<LogInRequestResult> LogIn(LogInRequest param)
    {
        _logger.LogInformation("Login {Param}", param);
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == param.Id);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.DeviceType == (DeviceType)param.DeviceType && i.DeviceId == param.DeviceId);
            if(playerDevice is not null) {
                var utcNow = DateTime.UtcNow;
                if (playerAccount.CurrentDeviceId > 0 && playerAccount.CurrentDeviceId != playerDevice.Id && utcNow < _jwt.ExpireDate(playerDevice.LastUsed.Value)) {
                    // It will deny that last token not expired yet and login with other device.
                    _logger.LogInformation("already logged in with other device. overwrite.");
                }
                if(!string.IsNullOrEmpty(param.NewDeviceId) && param.NewDeviceId != param.DeviceId) {
                    playerDevice = new PlayerDevice {
                        OwnerId = playerAccount.Id,
                        DeviceType = (DeviceType)param.DeviceType,
                        DeviceId = param.NewDeviceId,
                        Since = utcNow,
                        LastUsed = utcNow,
                    };
                    await _dbContext.AddAsync(playerDevice);
                    await _dbContext.SaveChangesAsync();
                }
                playerDevice.LastUsed = playerAccount.LastLogin = utcNow;
                playerAccount.CurrentDeviceId = playerDevice.Id;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("{PlayerId}|Login {DeviceId}", playerAccount.Id, playerDevice.Id);
                return new LogInRequestResult {
                    Token = $"Bearer {_jwt.CreateToken(playerAccount.Id, playerDevice.Id, out var period)}",
                    Period = period,
                    Code = playerAccount.Code
                };
            }
        }
        throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
    }

    /// <summary>
    /// Request new token
    /// </summary>
    /// <returns>new token</returns>
    [FromTypeFilter(typeof(VerifyTokenAndAccount))]
    public async UnaryResult<RenewTokenRequestResult> RenewToken()
    {
        _httpContextAccessor.TryGetPlayerIdAndDeviceId(out var playerId, out var deviceId);
        _logger.LogInformation("{PlayerId}|RenewToken {DeviceId}", playerId, deviceId);
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Id == deviceId);
            if(playerDevice is not null) {
                if(playerAccount.CurrentDeviceId != deviceId) {
                    throw new ReturnStatusException(StatusCode.FailedPrecondition, "You are not logged in with current device.");
                }
                playerDevice.LastUsed = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return new RenewTokenRequestResult {
                    Token = $"Bearer {_jwt.CreateToken(playerId, deviceId, out var period)}",
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
    [FromTypeFilter(typeof(VerifyTokenAndAccount))]
    public async UnaryResult<Nil> LogOut()
    {
        _httpContextAccessor.TryGetPlayerIdAndDeviceId(out var playerId, out var deviceId);
        _logger.LogInformation("{PlayerId}|LogOut {DeviceId}", playerId, deviceId);
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Id == deviceId);
            if(playerDevice is not null) {
                if(playerAccount.CurrentDeviceId != deviceId) {
                    throw new ReturnStatusException(StatusCode.FailedPrecondition, "You are not logged in with current device.");
                }
                playerAccount.CurrentDeviceId = 0;
                await _dbContext.SaveChangesAsync();
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
    public async UnaryResult<SignUpRequestResult> SignUp(SignUpRequest signup)
    {
        _logger.LogInformation("SignUp {SignUp}", signup);
        if(!string.IsNullOrWhiteSpace(signup.DeviceId)) {
            var playerAccount = await CreateAccountAsync(_dbContext, signup);
            return new SignUpRequestResult {
                Token = $"Bearer {_jwt.CreateToken(playerAccount.Id, playerAccount.CurrentDeviceId, out var period)}",
                Period = period,
                Id = playerAccount.Id,
                Code = playerAccount.Code
            };
        }
        throw new ReturnStatusException(StatusCode.InvalidArgument, "Device Identifier is invalid.");
    }

    public static async Task<PlayerAccount> CreateAccountAsync(GameDbContext context, SignUpRequest accountCreationModel)
    {
        return await AccountOperation.CreateAccountAsync(context, (DeviceType)accountCreationModel.DeviceType, accountCreationModel.DeviceId);
    }
}
