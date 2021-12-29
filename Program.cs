using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MessagePack;
using MessagePack.AspNetCoreMvcFormatter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning(options => {
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

// https://stackoverflow.com/questions/4804086/is-there-any-connection-string-parser-in-c

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var csBulder = new DbConnectionStringBuilder { ConnectionString = connectionString };
// if(csBulder["server"] == null) {
builder.Services.AddDbContext<YourGameServer.Data.GameDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// }
// else {
//     // CPomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb // 最新でこの定義の使い所がないがいいのか？
//     builder.Services.AddDbContext<GameDbContext>(options => options.UseMySql(csBulder.ConnectionString, new MySqlServerVersion(new Version(10, 6, 5))));
// }
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(option => {
    var msgpackOption = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
    option.OutputFormatters.Add(new MessagePackOutputFormatter(msgpackOption));
    option.InputFormatters.Add(new MessagePackInputFormatter(msgpackOption));
    // Mapモードの場合。アノテーションが要らなくなるのはいいが、メッセージが太るのでMessagepack使う意義が薄れると思う
    // var msgpackOption = MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);
    // option.OutputFormatters.Add(new MessagePackOutputFormatter(msgpackOption));
    // option.InputFormatters.Add(new MessagePackInputFormatter(msgpackOption));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
