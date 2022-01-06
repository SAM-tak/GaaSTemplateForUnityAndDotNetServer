using System;
using System.IO;
using System.Security;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using YourGameServer;
using YourGameServer.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddApiVersioning(options => {
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

var jwtTokenGenarator = new JwtTokenGenarator(builder.Configuration);
if(builder.Environment.IsProduction() && jwtTokenGenarator.ExpireMinutes <= 0) throw new SecurityException("Jwt.ExpireMinute must be over 0 in production.");
builder.Services.AddSingleton(i => jwtTokenGenarator);
//builder.AddJwtTokenGenarator();

// https://stackoverflow.com/questions/4804086/is-there-any-connection-string-parser-in-c

var connectionString = builder.Configuration.GetConnectionString(builder.Configuration["GameDbConnectionString"]);
var dbcsb = new DbConnectionStringBuilder() { ConnectionString = connectionString };
if(dbcsb.ContainsKey("Data Source") && Path.GetExtension(dbcsb["Data Source"].ToString()) == ".db") {
    builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite(connectionString));
}
else {
    // CPomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb // 最新でこの定義の使い所がないがいいのか？
    builder.Services.AddDbContext<GameDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 6, 5))));
}
// Add services to the container.
builder.Services.AddAuthentication(options => {
    if(builder.Environment.IsDevelopment()) {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    }
})
.AddCookie()
.AddCookie("OpenIdConnect")
.AddGoogle(options => {
    IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuthNSection["ClientId"];
    options.ClientSecret = googleAuthNSection["ClientSecret"];
    options.SaveTokens = true;
})
.AddJwtBearer(options => options.TokenValidationParameters = jwtTokenGenarator.TokenValidationParameters)
//.AddMicrosoftIdentityWebApp(builder.Configuration)
;
//builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, ApiAuthHandler>("Api", null);
builder.Services.AddAuthorization(options => {
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy("AllowOtherPlayer", new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
});

builder.Services.AddControllersWithViews(options => {
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
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

jwtTokenGenarator._serviceProvider = app.Services;
//app.UseJwtTokenGenarator();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}
if(app.Environment.IsProduction()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapRazorPages();
app.MapBlazorHub();

// app.UseRewriter(new RewriteOptions().Add(context => { // AddMicrosoftIdentityUI使うとこんなことしなきゃいけなくなる
//     if (context.HttpContext.Request.Path == "/MicrosoftIdentity/Account/SignOut") {
//         context.HttpContext.Response.Redirect("/");
//     }
// }));

app.Run();
