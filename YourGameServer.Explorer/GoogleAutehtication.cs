using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Explorer.Data;
using YourGameServer.Explorer.Models;

namespace YourGameServer.Explorer;

public static class GoogleAuthentication
{
    public static void SetUpOption(GoogleOptions options, IConfigurationSection googleAuthNSection)
    {
        options.ClientId = googleAuthNSection["ClientId"]
            ?? throw new InvalidOperationException("Google OAuth 'ClientId' not found.");
        options.ClientSecret = googleAuthNSection["ClientSecret"]
            ?? throw new InvalidOperationException("Google OAuth 'ClientSecret' not found.");
        options.Events = new OAuthEvents {
            OnCreatingTicket = async context => {
                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                // Process something with ExplorerDbContext
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<ExplorerDbContext>();
                // Example: Add a new user to the database
                var nameIdentifier = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(nameIdentifier != null) {
                    var user = await dbContext.Users.Include(x => x.RoleAssigns).FirstOrDefaultAsync(x => x.NameIdentifier == nameIdentifier);
                    if(user == null) {
                        var isFirst = !await dbContext.Users.AnyAsync();
                        user = new User {
                            NameIdentifier = nameIdentifier,
                            RoleAssigns = [
                                new() {
                                NameIdentifier = nameIdentifier,
                                Role = isFirst ? UserRole.Admin : UserRole.Guest,
                            }
                            ],
                            Name = identity.Name!,
                            EmailAddress = context.Principal?.FindFirst(ClaimTypes.Email)?.Value,
                            Since = DateTime.UtcNow,
                            LastLogin = DateTime.UtcNow,
                        };
                        await dbContext.Users.AddAsync(user);
                        await dbContext.SaveChangesAsync();
                    }
                    else {
                        user.LastLogin = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync();
                    }
                    if(user != null && user.RoleAssigns?.Count > 0) {
                        // Add custom role claim
                        foreach(var role in user.RoleAssigns) {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role.Role.ToString()));
                        }
                    }
                }
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("User {User} ({Role}) logged in.",
                    context.Principal?.Identity?.Name,
                    context.Principal?.FindFirst(ClaimTypes.Role)?.Value);
            }
        };
        options.SaveTokens = true;
    }
}
