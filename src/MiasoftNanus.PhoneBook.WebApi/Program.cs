using MiasoftNanus.PhoneBook.WebApi.Extensions;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

#region Serilog configuration
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "log.txt");
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();
#endregion

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
    builder.Services.AddApplicationConfigurationServices(builder.Configuration);
    builder.Services.AddApplicationLayersServices(builder.Configuration);
    #endregion
    
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (builder.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        await app.MigrateDatabaseAsync();
    }

    app.UseHttpsRedirection();
    app.MapEndpoints();

    await app.RunAsync();
}
catch (Exception ex)
{
    logger.Fatal(ex, "An unhandled exception has occurred in the middleware of the application");
}
finally
{
    await Log.CloseAndFlushAsync();
}