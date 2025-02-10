#nullable disable
using Cysharp.Runtime.Multicast;
using MagicOnion.Server.Hubs;
using YourGameServer.Game.Interface;

namespace YourGameServer.Game.Services;

public class ChatHub(IMulticastGroupProvider groupProvider) : StreamingHubBase<IChatHub, IChatHubReceiver>, IChatHub
{
    readonly IMulticastSyncGroup<Guid, IChatHubReceiver> _roomForAll = groupProvider.GetOrAddSynchronousGroup<Guid, IChatHubReceiver>("All");

    IGroup<IChatHubReceiver> _room;

    string _userName = string.Empty;

    public async Task JoinAsync(ChatJoinRequest request)
    {
        _room = await Group.AddAsync(request.RoomName);
        _userName = request.UserName;
        _room.All.OnJoin(request.UserName);
    }

    public async Task LeaveAsync()
    {
        if(_room is not null) {
            await _room.RemoveAsync(Context);
            _room.All.OnLeave(_userName);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if(_room is not null) {
            if(message.StartsWith("/global ", StringComparison.InvariantCultureIgnoreCase)) {
                var response = new ChatMessage { UserName = _userName, Message = message["/global ".Length..] };
                _roomForAll.All.OnSendMessage(response);
            }
            else {
                var response = new ChatMessage { UserName = _userName, Message = message };
                _room.All.OnSendMessage(response);
            }
        }

        await Task.CompletedTask;
    }

    public Task GenerateException(string message)
    {
        throw new Exception(message);
    }

    // It is not called because it is a method as a sample of arguments.
    public Task SampleMethod(List<int> sampleList, Dictionary<int, string> sampleDictionary)
    {
        throw new NotImplementedException();
    }

    protected override ValueTask OnConnecting()
    {
        // handle connection if needed.
        Console.WriteLine($"client connected {Context.ContextId}");
        _roomForAll.Add(ConnectionId, Client);
        return CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        // handle disconnection if needed.
        // on disconnecting, if automatically removed this connection from group.
        _roomForAll.Remove(ConnectionId);
        return CompletedTask;
    }
}
