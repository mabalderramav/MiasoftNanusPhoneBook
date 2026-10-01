using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MiasoftNanus.PhoneBook.WebApi.Extensions;

public static class AppLoggingBuilderExtensions
{
    public static Logger InitLogging(this ILoggingBuilder builder)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "log.txt");
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            .CreateLogger();
        
        builder.AddSerilog(logger);
        return logger;
    }
}