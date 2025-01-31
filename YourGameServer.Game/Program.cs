using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using MagicOnion.Server;
using NLog;
using NLog.Web;
using YourGameServer.Shared;
using YourGameServer.Shared.Data;
using YourGameServer.Game;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try {
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    builder.Services.AddHttpContextAccessor();

    // Setup IDCoder(hashids)
    IDCoder.Initialize(builder.Configuration.GetSection("IDCoder")["Salt"] ?? string.Empty);

    // https://stackoverflow.com/questions/4804086/is-there-any-connection-string-parser-in-c
    var connectionString = builder.Configuration.GetConnectionString(builder.Configuration["GameDbConnectionStringKey"] ?? string.Empty);
    var dbcsb = new DbConnectionStringBuilder() { ConnectionString = connectionString };
    if(dbcsb.ContainsKey("Data Source") && Path.GetExtension(dbcsb["Data Source"].ToString()) == ".db") {
        builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite(connectionString));
    }
    else {
        builder.Services.AddDbContext<GameDbContext>(options => options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 5))));
    }
    // Add services to the container.
    var authentication = builder.Services.AddAuthentication(options => {
        if(builder.Environment.IsDevelopment()) {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        }
    });
    if(builder.Environment.IsDevelopment()) {
        authentication.AddCookie(); // this is neccesary
        authentication.AddCookie("OpenIdConnect");
        authentication.AddGoogle(options => {
            IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
            options.ClientId = googleAuthNSection["ClientId"] ?? string.Empty;
            options.ClientSecret = googleAuthNSection["ClientSecret"] ?? string.Empty;
            options.SaveTokens = true;
        });
    }
    authentication.AddJwtAuthorizer(builder);
    //authentication.AddMicrosoftIdentityWebApp(builder.Configuration);

    //builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ApiAuthHandler>("Api", null);
    _ = builder.Services.AddAuthorization(options => {
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        options.AddPolicy("AllowOtherPlayer", new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
    });

    builder.Services.AddGrpc();
    builder.Services.AddMagicOnion();

    var app = builder.Build();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseJwtAuthorizer();
    app.MapMagicOnionService().AllowAnonymous();
    // Configure the HTTP request pipeline.
    if(app.Environment.IsDevelopment()) {
        app.UseGrpcWeb();
        var methodHandlers = app.Services.GetService<MagicOnionServiceDefinition>()?.MethodHandlers;
        // var url = builder.Configuration.GetSection("Kestrel:Endpoints:Grpc")?.GetValue<string>("Url");
        // Console.WriteLine($"Kestrel:Endpoints:Grpc = {url}");
        // app.MapMagicOnionHttpGateway("rpcswagger", methodHandlers!, GrpcChannel.ForAddress("https://localhost/rpc"));
        app.MapMagicOnionSwagger("rpcswagger", methodHandlers!, "/_/");
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
