namespace YourGameServer.Game.Extensions;

public static class IHttpContextAccessorExtension
{
    public static bool TryGetPlayerIdAndDeviceIdx(this IHttpContextAccessor httpContextAccessor, out ulong playerId, out int deviceIdx)
    {
        // Claims.Last "aud: YourGameClient/<PlayerID>/<DeviceID>"
        if(httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            var strings = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "aud")?.Value.Split('/');
            if(strings != null && strings.Length > 2 && ulong.TryParse(strings[1], out playerId) && int.TryParse(strings[2], out deviceIdx)) {
                return true;
            }
        }
        playerId = 0;
        deviceIdx = 0;
        return false;
    }

    public static ulong GetPlayerId(this IHttpContextAccessor httpContextAccessor)
    {
        // Claims.Last "aud: YourGameClient/<PlayerID>/<DeviceID>"
        if(httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            var strings = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "aud")?.Value.Split('/');
            if(strings != null && strings.Length > 2 && ulong.TryParse(strings[1], out var playerId)) {
                return playerId;
            }
        }
        return 0;
    }
}
