using MessagePack.AspNetCoreMvcFormatter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(option => {
    option.OutputFormatters.Add(new MessagePackOutputFormatter());
    option.InputFormatters.Add(new MessagePackInputFormatter());
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
