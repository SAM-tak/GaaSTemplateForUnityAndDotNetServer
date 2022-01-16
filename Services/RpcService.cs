using System;
using System.ComponentModel;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Interface;
using YourGameServer.Models;

namespace YourGameServer.Services;

// Implements RPC service in the server project.
// The implementation class must inherit `ServiceBase<IMyFirstService>` and `IMyFirstService`
public class RpcService : ServiceBase<IRpcService>, IRpcService
{
    readonly GameDbContext _context;
    readonly JwtTokenGenarator _jwt;
    public RpcService(GameDbContext context, JwtTokenGenarator jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    // `UnaryResult<T>` allows the method to be treated as `async` method.
    public async UnaryResult<TokenRequestResult?> Login(TokenRequest param)
    {
        var playerAccount = await _context.PlayerAccounts.Include(i => i.DeviceList).FirstOrDefaultAsync(i => i.Id == param.Id);
        if(playerAccount != null) {
            var playerDevice = playerAccount.DeviceList.FirstOrDefault(i => i.DeviceType == param.DeviceType && i.DeviceId == param.DeviceId);
            if(playerDevice != null) {
                playerAccount.LastLogin = DateTime.UtcNow;
                if(!string.IsNullOrEmpty(param.NewDeviceId) && param.NewDeviceId != param.DeviceId) {
                    playerDevice = new PlayerDevice {
                        DeviceType = param.DeviceType,
                        DeviceId = param.NewDeviceId,
                        Since = playerAccount.LastLogin,
                        LastUsed = playerAccount.LastLogin,
                    };
                    playerAccount.DeviceList.Add(playerDevice);
                }
                playerDevice.LastUsed = playerAccount.LastLogin;
                playerAccount.CurrentDeviceId = playerDevice.Id;
                _context.Entry(playerAccount).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return new TokenRequestResult {
                    DeviceId = playerDevice.Id,
                    Token = _jwt.CreateToken(playerAccount.Id, playerDevice.Id)
                };
            }
        }
        return null;
    }
}
