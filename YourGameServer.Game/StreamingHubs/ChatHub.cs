#nullable disable
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using YourGameServer.Game.Extensions;
using YourGameServer.Game.Interface;

namespace YourGameServer.Game.Services;

public class ChatHub(IHttpContextAccessor httpContextAccessor, ILogger<ChatHub> logger) : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly ILogger<ChatHub> _logger = logger;
    IGroup _room;

    string _userName = string.Empty;

    [FromTypeFilter(typeof(VerifyToken))]
    public async Task<Guid> JoinAsync(ChatJoinRequest request)
    {
        _logger.LogInformation("{PlayerId}|JoinAsync {UserName} {RoomName}", _httpContextAccessor.GetPlayerId(), request.UserName, request.RoomName);
        _room = await Group.AddAsync(request.RoomName);
        _userName = request.UserName;
        Broadcast(_room).OnJoin(new() { UserName = _userName, ContextId = Context.ContextId });
        _logger.LogInformation("{PlayerId}|JoinAsync done {UserName} {RoomName}", _httpContextAccessor.GetPlayerId(), _userName, _room.GroupName);
        return Context.ContextId;
    }

    [FromTypeFilter(typeof(VerifyToken))]
    public async Task LeaveAsync()
    {
        if(_room is not null) {
            await _room.RemoveAsync(Context);
            Broadcast(_room).OnLeave(new() { UserName = _userName, ContextId = Context.ContextId });
        }
    }

    [FromTypeFilter(typeof(VerifyToken))]
    public async Task SendMessageAsync(string message)
    {
        if(_room is not null) {
            if(message.StartsWith("/global ", StringComparison.InvariantCultureIgnoreCase)) {
                Broadcast(_room).OnRecievedMessage(new() {
                    Member = new() { UserName = _userName, ContextId = Context.ContextId },
                    DateTime = DateTime.UtcNow,
                    Message = message["/global ".Length..]
                });
            }
            else {
                Broadcast(_room).OnRecievedMessage(new() {
                    Member = new() { UserName = _userName, ContextId = Context.ContextId },
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
        return CompletedTask;
    }
}
