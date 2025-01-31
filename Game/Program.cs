using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using MagicOnion.Server;
using NLog;
using NLog.Web;
using YourGameServer.Shared;
using YourGameServer.Shared.Data;

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

    builder.Services.AddControllers(options => {
        var msgpackOption = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        options.OutputFormatters.Add(new MessagePackOutputFormatter(msgpackOption));
        options.InputFormatters.Add(new MessagePackInputFormatter(msgpackOption));
        // Mapモードの場合。アノテーションが要らなくなるのはいいが、メッセージが太るのでMessagepack使う意義が薄れると思う
        // var msgpackOption = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);
        // option.OutputFormatters.Add(new MessagePackOutputFormatter(msgpackOption));
        // option.InputFormatters.Add(new MessagePackInputFormatter(msgpackOption));
    })
    //.AddMicrosoftIdentityUI() // 今のところ役に立ってない
    ;
    if(builder.Environment.IsDevelopment()) {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    builder.Services.AddGrpc();
    builder.Services.AddMagicOnion();

    var app = builder.Build();

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseJwtAuthorizer();
    app.MapMagicOnionService().AllowAnonymous();
    // Configure the HTTP request pipeline.
    if(app.Environment.IsDevelopment()) {
        app.MapBlazorHub();
        app.UseGrpcWeb();
        app.MapFallbackToPage("/_Host");
        app.UseSwagger();
        app.UseSwaggerUI();
        var methodHandlers = app.Services.GetService<MagicOnionServiceDefinition>()?.MethodHandlers;
        // var url = builder.Configuration.GetSection("Kestrel:Endpoints:Grpc")?.GetValue<string>("Url");
        // Console.WriteLine($"Kestrel:Endpoints:Grpc = {url}");
        // app.MapMagicOnionHttpGateway("rpcswagger", methodHandlers!, GrpcChannel.ForAddress("https://localhost/rpc"));
        app.MapMagicOnionSwagger("rpcswagger", methodHandlers!, "/_/");
    }
    else {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        app.UseHttpsRedirection();
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
