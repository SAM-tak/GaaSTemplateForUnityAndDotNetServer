using Microsoft.AspNetCore.Components;

namespace YourGameServer.Explorer.Extensions;

static class NavigationManagerExtension
{
    public static string ToLocalBasePathComponent(this NavigationManager self)
    {
        var localPath = self.Uri[self.BaseUri.Length..].ToLower().Split('/').FirstOrDefault();
        return localPath?.StartsWith('/') ?? false ? localPath : $"/{localPath}";
    }

    public static string BasePath(this NavigationManager self) => self.ToLocalBasePathComponent().Split('?').First();
}
