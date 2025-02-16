using System.Data.Common;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Google;
using MudBlazor.Services;
using NLog;
using NLog.Web;
using YourGameServer.Shared;
using YourGameServer.Shared.Data;
using YourGameServer.Explorer.Components;
using YourGameServer.Explorer.Services;
using YourGameServer.Explorer.Data;
using YourGameServer.Explorer.Models;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try {
    var builder = WebApplication.CreateBuilder(args);

    if(!builder.Environment.IsDevelopment()) {
        // AWS Secrets Manager
        // builder.Configuration.AddSecretsManager();

        // Azure Key Vault
        // var azureKeyVault = builder.Configuration.GetSection("AzureKeyVault")
        //     ?? throw new InvalidOperationException("Azure Key Vault Settings not found.");
        // builder.Configuration.AddAzureKeyVault(
        //     new Uri($"https://{azureKeyVault["Name"] ?? throw new InvalidOperationException("Azure Key Vault Name not found.")}.vault.azure.net/"),
        //     new DefaultAzureCredential());

        // HashiCorp Vault
        // var hashiCorpVault = builder.Configuration.GetSection("HashiCorpVault")
        //     ?? throw new InvalidOperationException("HashiCorp Vault Settings not found.");
        // builder.Configuration.AddVault(
        //     options => {
        //         options.Address = $"https://{hashiCorpVault["Address"] ?? throw new InvalidOperationException("HashiCorp Vault Address not found.")}:8200";
        //         options.Token = $"{hashiCorpVault["Token"] ?? throw new InvalidOperationException("HashiCorp Vault Token not found.")}";
        //     });
    }

    // Add service defaults & Aspire components.
    builder.AddServiceDefaults();

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // Set up for log view component
    builder.Services.AddSingleton<ILogMonitorService, LogMonitorService>();

    builder.Services.AddHttpContextAccessor();

    // Setup IDCoder(sqids)
    IDCoder.Initialize();

    // https://stackoverflow.com/questions/4804086/is-there-any-connection-string-parser-in-c
    {
        var connectionString = builder.Configuration.GetConnectionString("GameDbConnection")
            ?? throw new InvalidOperationException("Connection string 'GameDbConnection' not found.");
        var dbcsb = new DbConnectionStringBuilder() { ConnectionString = connectionString };
        if((dbcsb.ContainsKey("DataSource") && Path.GetExtension(dbcsb["DataSource"].ToString()) == ".db")
        || (dbcsb.ContainsKey("Data Source") && Path.GetExtension(dbcsb["Data Source"].ToString()) == ".db")) {
            builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite(connectionString));
        }
        else {
            builder.Services.AddDbContext<GameDbContext>(options => options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 5))));
        }
    }
    {
        var connectionString = builder.Configuration.GetConnectionString("ExplorerDbConnection")
            ?? throw new InvalidOperationException("Connection string 'ExplorerDbConnection' not found.");
        var dbcsb = new DbConnectionStringBuilder() { ConnectionString = connectionString };
        if((dbcsb.ContainsKey("DataSource") && Path.GetExtension(dbcsb["DataSource"].ToString()) == ".db")
        || (dbcsb.ContainsKey("Data Source") && Path.GetExtension(dbcsb["Data Source"].ToString()) == ".db")) {
            builder.Services.AddDbContext<ExplorerDbContext>(options => options.UseSqlite(connectionString));
        }
        else {
            builder.Services.AddDbContext<ExplorerDbContext>(options => options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 5))));
        }
    }

    // Add services to the container.
    _ = builder.Services
        .AddAuthentication(options => {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie("OpenIdConnect") // needs for log out process
        .AddGoogle(options => {
            IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
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
                        var user = await dbContext.ExplorerUsers.Include(x => x.RoleAssigns).FirstOrDefaultAsync(x => x.NameIdentifier == nameIdentifier);
                        if(user == null) {
                            bool isFirst = !await dbContext.ExplorerUsers.AnyAsync();
                            user = new ExplorerUser {
                                NameIdentifier = nameIdentifier,
                                RoleAssigns = [
                                    new() {
                                        NameIdentifier = nameIdentifier,
                                        Role = isFirst ? ExplorerUserRole.Admin : ExplorerUserRole.Guest,
                                    }
                                ],
                                Name = identity.Name!,
                                EmailAddress = context.Principal?.FindFirst(ClaimTypes.Email)?.Value,
                                Since = DateTime.UtcNow,
                                LastLogin = DateTime.UtcNow,
                            };
                            await dbContext.ExplorerUsers.AddAsync(user);
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
        });

    _ = builder.Services.AddAuthorization(options => {
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    });

    builder.Services.AddCascadingAuthenticationState();

    // Add MudBlazor services
    builder.Services.AddMudServices();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddControllers();

    var app = builder.Build();

    app.UseStaticFiles();
    app.UseCookiePolicy();
    app.UseAuthentication();
    app.UseAuthorization();

    // Configure the HTTP request pipeline.
    app.UseHttpsRedirection();
    app.UseAntiforgery();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
    app.MapDefaultControllerRoute();
    if(!app.Environment.IsDevelopment()) {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.Run();
}
catch(Exception exception) {
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally {
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}
