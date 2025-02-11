#nullable disable
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Game.Extensions;
using YourGameServer.Game.Interface;
using YourGameServer.Shared.Data;

namespace YourGameServer.Game.Services;

public class ChatHub(GameDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<ChatHub> logger) : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    readonly GameDbContext _dbContext = dbContext;
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<ChatHub> _logger = logger;
    IGroup _room;

    string _playerCode = string.Empty;
    string _playerName = string.Empty;

    static HashSet<ulong> _joinedPlayerId = [];

    [FromTypeFilter(typeof(VerifyToken))]
    public async Task JoinAsync(string roomName)
    {
        var playerId = _httpContextAccessor.GetPlayerId();
        if(_joinedPlayerId.Contains(playerId)) {
            throw new ReturnStatusException(StatusCode.AlreadyExists, "You are already joined chat.");
        }
        var playerAccount = await _dbContext.PlayerAccounts.Include(i => i.Profile).FirstOrDefaultAsync(i => i.Id == playerId);
        _room = await Group.AddAsync(roomName);
        var i = await _room.GetMemberCountAsync();
        _playerCode = playerAccount.Code;
        _playerName = playerAccount.Profile.Name;
        _joinedPlayerId.Add(playerId);
        Broadcast(_room).OnJoin(new() { PlayerName = _playerName, PlayerCode = _playerCode });
        _logger.LogInformation("{PlayerId}|JoinAsync {PlayerCode} {RoomName}", _httpContextAccessor.GetPlayerId(), _playerCode, _room.GroupName);
    }

    [FromTypeFilter(typeof(VerifyToken))]
    public async Task LeaveAsync()
    {
        if(_room is not null) {
            await _room.RemoveAsync(Context);
            _joinedPlayerId.Remove(_httpContextAccessor.GetPlayerId());
            Broadcast(_room).OnLeave(new() { PlayerName = _playerName, PlayerCode = _playerCode });
        }
    }

    [FromTypeFilter(typeof(VerifyToken))]
    public async Task SendMessageAsync(string message)
    {
        if(_room is not null) {
            if(message.StartsWith("/global ", StringComparison.InvariantCultureIgnoreCase)) {
                Broadcast(_room).OnRecievedMessage(new() {
                    Member = new() { PlayerName = _playerName, PlayerCode = _playerCode },
                    DateTime = DateTime.UtcNow,
                    Message = message["/global ".Length..]
                });
            }
            else {
                Broadcast(_room).OnRecievedMessage(new() {
                    Member = new() { PlayerName = _playerName, PlayerCode = _playerCode },
                    DateTime = DateTime.UtcNow,
                    Message = message
                });
            }
        }

        await Task.CompletedTask;
    }

    [FromTypeFilter(typeof(VerifyTokenAndAccount))]
    protected override ValueTask OnConnecting()
    {
        // handle connection if needed.
        _logger.LogInformation("{PlayerId}|client connected {ContextId}", _httpContextAccessor.GetPlayerId(), Context.ContextId);
        // _roomForAll.Add(ConnectionId, Client);
        return CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        // handle disconnection if needed.
        _logger.LogInformation("{PlayerId}|client disconnected {ContextId}", _httpContextAccessor.GetPlayerId(), Context.ContextId);
        // on disconnecting, if automatically removed this connection from group.
        // _roomForAll.Remove(ConnectionId);
        _joinedPlayerId.Remove(_httpContextAccessor.GetPlayerId());
        return CompletedTask;
    }
}
