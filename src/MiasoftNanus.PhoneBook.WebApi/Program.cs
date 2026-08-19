using MiasoftNanus.PhoneBook.WebApi.Config;
using MiasoftNanus.PhoneBook.WebApi.Endpoints;
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
    #region Logger
    builder.Logging.AddSerilog(logger);
    logger.Information(
        "LOG INITIALIZED in {GetEnvironmentVariable}",
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "ENVIRONMENT NOT DEFINED.");
    #endregion

    #region Services
    builder.Services.AddOpenApi();
    builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("API"));
    #endregion
    
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (builder.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    #region Endpoints
    app.MapGet("/", () => "Hello World! - Oscar Martin Balderrama Vaca - Miasoft Nanus PhoneBook API");
    app.MapHealthEndpoints();
    #endregion

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