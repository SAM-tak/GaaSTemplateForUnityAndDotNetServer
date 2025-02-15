using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MudBlazor.Services;
using NLog;
using NLog.Web;
using YourGameServer.Shared;
using YourGameServer.Shared.Data;
using YourGameServer.Explorer.Components;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try {
    var builder = WebApplication.CreateBuilder(args);

    // Add service defaults & Aspire components.
    builder.AddServiceDefaults();

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    builder.Services.AddHttpContextAccessor();

    // Setup IDCoder(sqids)
    IDCoder.Initialize();

    // https://stackoverflow.com/questions/4804086/is-there-any-connection-string-parser-in-c
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    var dbcsb = new DbConnectionStringBuilder() { ConnectionString = connectionString };
    if((dbcsb.ContainsKey("DataSource") && Path.GetExtension(dbcsb["DataSource"].ToString()) == ".db")
     || (dbcsb.ContainsKey("Data Source") && Path.GetExtension(dbcsb["Data Source"].ToString()) == ".db")) {
        builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite(connectionString));
    }
    else {
        builder.Services.AddDbContext<GameDbContext>(options => options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 5))));
    }

    // Add services to the container.
    var authentication = builder.Services
        .AddAuthentication(options => {
            if(builder.Environment.IsDevelopment()) {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            }
        })
        .AddCookie() // this is neccesary
        .AddCookie("OpenIdConnect");

    if(builder.Environment.IsDevelopment()) {
        authentication.AddGoogle(options => {
            IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
            options.ClientId = googleAuthNSection["ClientId"]
                ?? throw new InvalidOperationException("Google OAuth 'ClientId' not found.");
            options.ClientSecret = googleAuthNSection["ClientSecret"]
                ?? throw new InvalidOperationException("Google OAuth 'ClientSecret' not found.");
            options.SaveTokens = true;
        });
    }

    builder.Services.AddAuthorization();

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

    app.MapDefaultControllerRoute();

    // Configure the HTTP request pipeline.
    if(!app.Environment.IsDevelopment()) {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

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
