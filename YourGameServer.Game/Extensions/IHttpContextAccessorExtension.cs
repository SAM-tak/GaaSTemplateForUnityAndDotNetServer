namespace YourGameServer.Game.Extensions;

public static class IHttpContextAccessorExtension
{
    public static bool TryGetPlayerIdAndDeviceId(this IHttpContextAccessor httpContextAccessor, out ulong playerId, out ulong deviceId)
    {
        // Claims.Last "aud: YourGameClient/<PlayerID>/<DeviceID>"
        if(httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            var strings = httpContextAccessor.HttpContext.User.Claims.Last().Value[5..].Split('/');
            if(strings.Length > 2 && ulong.TryParse(strings[1], out playerId) && ulong.TryParse(strings[2], out deviceId)) {
                return true;
            }
        }
        playerId = deviceId = 0;
        return false;
    }

    public static ulong GetPlayerId(this IHttpContextAccessor httpContextAccessor)
    {
        // Claims.Last "aud: YourGameClient/<PlayerID>/<DeviceID>"
        if(httpContextAccessor != null && httpContextAccessor.HttpContext != null) {
            var strings = httpContextAccessor.HttpContext.User.Claims.Last().Value[5..].Split('/');
            if(strings.Length > 2 && ulong.TryParse(strings[1], out var playerId)) {
                return playerId;
            }
        }
        return 0;
    }
}
