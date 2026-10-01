using MiasoftNanus.PhoneBook.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MiasoftNanus.PhoneBook.WebApi.Extensions;

/// <summary>
/// Provides extension methods for managing database migrations within the application's startup pipeline.
/// </summary>
/// <remarks>
/// This class includes functionality to apply pending database migrations during application startup.
/// The migration process is executed within a scope created from the application's services, allowing
/// for dependency injection of required services such as <see cref="AppDbContext"/> and <see cref="ILoggerFactory"/>.
/// </remarks>
public static class DataBaseMigrationBuilderExtensions
{
    /// <summary>
    /// Applies any pending database migrations for the application's DbContext during the startup process.
    /// </summary>
    /// <param name="app">
    /// The <see cref="IApplicationBuilder"/> instance used to configure the middleware pipeline.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
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