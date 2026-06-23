using MiasoftNanus.PhoneBook.WebApi.Config;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("API"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World! - Oscar Martin Balderrama Vaca - Miasoft Nanus PhoneBook API");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/version", () => Results.Ok(new { version = "1.0.0" }));
app.MapGet("/info", () => Results.Ok(new
{
    name = "Miasoft Nanus PhoneBook API", 
    version = "1.0.0", 
    description = "A simple phone book API built with ASP.NET Core."
}));
app.MapGet("/status", () => Results.Ok(new { status = "Running", timestamp = DateTime.UtcNow }));
app.MapGet("/metrics", () => Results.Ok(new
{
    uptime = (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds,
    memoryUsage = GC.GetTotalMemory(false),
    threadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count
}));
app.MapGet("/Config", (IConfiguration configuration, IOptions<ApiConfig> apiConfigOptions) =>
{
    var apiConfig = apiConfigOptions.Value;
    var environment = configuration["ASPNETCORE_ENVIRONMENT"];
    var connectionString = configuration.GetConnectionString("ConnectionSql");
    var timeout = apiConfig.Timeout;
    var baseUrl = apiConfig.BaseUrl;
    return Results.Ok(new
    {
        environment,
        connectionString,
        timeout,
        baseUrl
    });
});

app.Run();
