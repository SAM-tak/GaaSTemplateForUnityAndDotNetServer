using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using MudBlazor.Services;
using MagicOnion.Server;
using YourGameServer;
using YourGameServer.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddApiVersioning(options => {
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

// https://stackoverflow.com/questions/4804086/is-there-any-connection-string-parser-in-c
var connectionString = builder.Configuration.GetConnectionString(builder.Configuration["GameDbConnectionStringKey"]);
var dbcsb = new DbConnectionStringBuilder() { ConnectionString = connectionString };
if(dbcsb.ContainsKey("Data Source") && Path.GetExtension(dbcsb["Data Source"].ToString()) == ".db") {
    builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite(connectionString));
}
else {
    // CPomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb // 最新でこの定義の使い所がないがいいのか？
    builder.Services.AddDbContext<GameDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 5))));
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
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
        options.SaveTokens = true;
    });
}
authentication.AddJwtAuthorizer(builder);
//authentication.AddMicrosoftIdentityWebApp(builder.Configuration);

//builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ApiAuthHandler>("Api", null);
builder.Services.AddAuthorization(options => {
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
builder.Services.AddMudServices();

builder.Services.AddGrpc();
builder.Services.AddMagicOnion(option => {
    option.SerializerOptions = option.SerializerOptions.WithCompression(MessagePackCompression.Lz4BlockArray);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseJwtAuthorizer();
app.MapDefaultControllerRoute();
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
