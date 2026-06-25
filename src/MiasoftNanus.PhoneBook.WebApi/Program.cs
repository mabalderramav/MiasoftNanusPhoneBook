using MiasoftNanus.PhoneBook.WebApi.Config;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "log.txt");
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

try
{
    builder.Logging.AddSerilog(logger);
    logger.Information(
        "LOG INITIALIZED in {GetEnvironmentVariable}",
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "ENVIRONMENT NOT DEFINED.");
    
    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("API"));
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (builder.Environment.IsDevelopment())
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
        uptime =
            (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds,
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
        var token = apiConfig.Token;
        return Results.Ok(new
        {
            environment,
            connectionString,
            timeout,
            baseUrl,
            token
        });
    });

    await app.RunAsync();
}
catch (Exception ex)
{
    logger.Fatal(ex, "An unhandled exception has occurred in the middleware of the application.");
}
finally
{
    await Log.CloseAndFlushAsync();
}