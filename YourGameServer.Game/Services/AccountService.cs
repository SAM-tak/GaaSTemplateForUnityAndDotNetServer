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
using Store = YourGameServer.Shared.Models.Store;

namespace YourGameServer.Game.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class AccountService(GameDbContext dbContext, JwtAuthorizer jwt, IHttpContextAccessor httpContextAccessor, IConfiguration config, ILogger<AccountService> logger)
    : ServiceBase<IAccountService>, IAccountService
{
    readonly GameDbContext _dbContext = dbContext;
    readonly IConfiguration _config = config;
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
            id = IDCoder.Decode(param.LoginKey);
        }
        catch(Exception e) {
            throw new ReturnStatusException(StatusCode.InvalidArgument, $"invalid login key. {e}");
        }

        var playerAccount = await _dbContext.PlayerAccounts.FindAsync(id)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        var playerDevice = _dbContext.PlayerDevices.FirstOrDefault(i => i.OwnerId == id && i.DeviceType == (DeviceType)param.DeviceType && i.DeviceIdentifier == param.DeviceIdentifier)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "specified existing device was not found.");
        if(playerDevice.DeviceType != (DeviceType)param.DeviceType) throw new ReturnStatusException(StatusCode.InvalidArgument, "device type was not match with existing one.");

        var utcNow = DateTime.UtcNow;
        if(playerAccount.CurrentDeviceIdx > 0 && playerAccount.CurrentDeviceIdx != playerDevice.Idx && utcNow < _jwt.ExpireDate(playerDevice.LastUsed.Value)) {
            // It will deny that last token not expired yet and login with other device.
            _logger.LogInformation("already logged in with other device. overwrite.");
        }
        if(!string.IsNullOrEmpty(param.NewDeviceIdentifier) && param.NewDeviceIdentifier != param.DeviceIdentifier) {
            using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var playerDevices = _dbContext.PlayerDevices.Where(x => x.OwnerId == id);
            var currentPlatformDevices = playerDevices.Where(x => x.DeviceType == (DeviceType)param.DeviceType);
            var currentPlatformDeviceCount = await currentPlatformDevices.CountAsync() + 1;
            // remove old devices if over capacity.
            var maxDeviceCountPerPlayer = _config.GetSection("YourGameServer")?.GetValue<int>("MaxDeviceCountPerPlayer") ?? 3;
            if(maxDeviceCountPerPlayer > 0 && maxDeviceCountPerPlayer < 3) maxDeviceCountPerPlayer = 3;
            if(maxDeviceCountPerPlayer > 0 && currentPlatformDeviceCount > maxDeviceCountPerPlayer) {
                _dbContext.PlayerDevices.RemoveRange(currentPlatformDevices.OrderBy(x => x.LastUsed).Take(currentPlatformDeviceCount - maxDeviceCountPerPlayer));
            }
            // add new device.
            playerDevice = new PlayerDevice {
                OwnerId = playerAccount.Id,
                Idx = await playerDevices.MaxAsync(x => x.Idx),
                DeviceType = (DeviceType)param.DeviceType,
                DeviceIdentifier = param.NewDeviceIdentifier,
                OfficialStore = playerDevice.OfficialStore,
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

    /// <summary>
    /// Request new token
    /// </summary>
    /// <returns>new token</returns>
    [FromTypeFilter(typeof(VerifyTokenAndAccount))]
    public async UnaryResult<RenewTokenRequestResult> RenewToken()
    {
        _httpContextAccessor.TryGetPlayerIdAndDeviceIdx(out var playerId, out var deviceIdx);
        _logger.LogInformation("{PlayerId}|RenewToken {DeviceIdx}", playerId, deviceIdx);
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Idx == deviceIdx)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "using device was not match.");
        playerDevice.LastUsed = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return new RenewTokenRequestResult {
            Token = $"Bearer {_jwt.CreateToken(playerId, deviceIdx, out var period)}",
            Period = period
        };
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
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == playerId)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.Idx == deviceIdx)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "using device was not match.");
        playerAccount.CurrentDeviceIdx = 0;
        await _dbContext.SaveChangesAsync();
        return new Nil();
    }

    /// <summary>
    /// Request renew secret
    /// </summary>
    /// <returns>new player code</returns>
    [FromTypeFilter(typeof(VerifyTokenAndAccount))]
    public async UnaryResult<string> RenewSecret()
    {
        var playerId = _httpContextAccessor.GetPlayerId();
        _logger.LogInformation("{PlayerId}|RenewSecret", playerId);
        var playerAccount = await PlayerAccountOperation.RenewSecret(_dbContext, playerId)
            ?? throw new ReturnStatusException(StatusCode.NotFound, "correspond account was not found.");
        await _dbContext.SaveChangesAsync();
        return playerAccount.Code;
    }

    /// <summary>
    /// Sign Up (Create New Account)
    /// </summary>
    /// <param name="signup"></param>
    /// <returns></returns>
    public async UnaryResult<SignUpRequestResult> SignUp(SignUpRequest signup)
    {
        _logger.LogInformation("SignUp {SignUp}", signup);
        if(string.IsNullOrWhiteSpace(signup.DeviceIdentifier)) throw new ReturnStatusException(StatusCode.InvalidArgument, "device Identifier was invalid.");
        var playerAccount = await PlayerAccountOperation.CreateAsync(_dbContext, (DeviceType)signup.DeviceType, (Store)signup.OfficialStore, signup.DeviceIdentifier);
        await _dbContext.SaveChangesAsync();
        return new SignUpRequestResult {
            Token = $"Bearer {_jwt.CreateToken(playerAccount.Id, playerAccount.CurrentDeviceIdx, out var period)}",
            Period = period,
            LoginKey = playerAccount.LoginKey,
            Code = playerAccount.Code
        };
    }
}
