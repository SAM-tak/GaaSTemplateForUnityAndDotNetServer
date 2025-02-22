using System.Security.Claims;

namespace YourGameServer.Explorer.Extensions;

public static class IHttpContextAccessorExtension
{
    public static string GetName(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "???";
    }

    public static string GetEmail(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "???@???";
    }

    public static string GetNameIdentifier(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor?.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "???";
    }

    public static string GetNameAndIdentifier(this IHttpContextAccessor httpContextAccessor)
    {
        return $"{httpContextAccessor.GetName()}({httpContextAccessor.GetNameIdentifier()})";
    }

    public static string GetNameAndEmail(this IHttpContextAccessor httpContextAccessor)
    {
        return $"{httpContextAccessor.GetName()}({httpContextAccessor.GetEmail()})";
    }
}
