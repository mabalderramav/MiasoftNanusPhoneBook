using MiasoftNanus.PhoneBook.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MiasoftNanus.PhoneBook.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task MigrateDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger(nameof(ApplicationBuilderExtensions));
            logger.LogError(ex, "An error occurred while migrating the database");
        }
    }
}