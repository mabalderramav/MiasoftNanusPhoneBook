using MiasoftNanus.PhoneBook.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MiasoftNanus.PhoneBook.WebApi.Extensions;

public static class DataBaseMigrationBuilderExtensions
{
    public static async Task MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(DataBaseMigrationBuilderExtensions));

        try
        {
            logger.LogInformation("Migrating the database");
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database");
        }
    }
}