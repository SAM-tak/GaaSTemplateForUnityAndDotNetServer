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
        ulong id = 0;
        try {
            id = IDCoder.DecodeFromLoginKey(param.LoginKey);
        }
        catch(Exception) {
            throw new ReturnStatusException(StatusCode.InvalidArgument, "invalid login key.");
        }
        var playerAccount = await _dbContext.PlayerAccounts.FindAsync(id);
        if(playerAccount is not null) {
            var playerDevice = _dbContext.PlayerDevices.FirstOrDefault(i => i.OwnerId == id && i.DeviceType == (DeviceType)param.DeviceType && i.DeviceId == param.DeviceId);
            if(playerDevice is not null) {
                var utcNow = DateTime.UtcNow;
                if (playerAccount.CurrentDeviceIdx > 0 && playerAccount.CurrentDeviceIdx != playerDevice.Idx && utcNow < _jwt.ExpireDate(playerDevice.LastUsed.Value)) {
                    // It will deny that last token not expired yet and login with other device.
                    _logger.LogInformation("already logged in with other device. overwrite.");
                }
                if(!string.IsNullOrEmpty(param.NewDeviceId) && param.NewDeviceId != param.DeviceId) {
                    using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
                    playerDevice = new PlayerDevice {
                        OwnerId = playerAccount.Id,
                        Idx = await _dbContext.PlayerDevices.Where(i => i.OwnerId == id).MaxAsync(x => x.Idx) + 1,
                        DeviceType = (DeviceType)param.DeviceType,
                        DeviceId = param.NewDeviceId,
                        Since = utcNow,
                        LastUsed = utcNow,
                    };
                    await _dbContext.AddAsync(playerDevice);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                playerDevice.LastUsed = playerAccount.LastLogin = utcNow;
                playerAccount.CurrentDeviceIdx = playerDevice.Idx;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("{PlayerId}|Login {DeviceIdx}", playerAccount.Id, playerDevice.Idx);
                return new LogInRequestResult {
                    Token = $"Bearer {_jwt.CreateToken(playerAccount.Id, playerDevice.Idx, out var period)}",
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
        _httpContextAccessor.TryGetPlayerIdAndDeviceIdx(out var playerId, out var deviceIdx);
        _logger.LogInformation("{PlayerId}|RenewToken {DeviceId}", playerId, deviceIdx);
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Idx == deviceIdx);
            if(playerDevice is not null) {
                playerDevice.LastUsed = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return new RenewTokenRequestResult {
                    Token = $"Bearer {_jwt.CreateToken(playerId, deviceIdx, out var period)}",
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
        _httpContextAccessor.TryGetPlayerIdAndDeviceIdx(out var playerId, out var deviceIdx);
        _logger.LogInformation("{PlayerId}|LogOut {DeviceIdx}", playerId, deviceIdx);
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId);
        if(playerAccount is not null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Idx == deviceIdx);
            if(playerDevice is not null) {
                playerAccount.CurrentDeviceIdx = 0;
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
                Token = $"Bearer {_jwt.CreateToken(playerAccount.Id, playerAccount.CurrentDeviceIdx, out var period)}",
                Period = period,
                LoginKey = playerAccount.LoginKey,
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
