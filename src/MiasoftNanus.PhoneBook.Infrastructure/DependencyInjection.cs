using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Application.Email;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;
using MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Email;
using MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Time;
using MiasoftNanus.PhoneBook.Infrastructure.Profiles;
using MiasoftNanus.PhoneBook.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MiasoftNanus.PhoneBook.Infrastructure;

/// <summary>
/// Provides extension methods for configuring and registering application infrastructure services.
/// </summary>
/// <remarks>
/// This class is responsible for setting up services related to database context, repositories,
/// and other core infrastructure dependencies required by the application.
/// It allows easy integration of infrastructure components into the Dependency Injection (DI) container.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Configures and registers the infrastructure layer dependencies for the application, including
    /// the database context, repositories, and various services.
    /// </summary>
    /// <param name="services">
    /// The collection of service descriptors where the dependencies will be registered.
    /// </param>
    /// <param name="configuration">
    /// An instance of <see cref="IConfiguration"/> used to retrieve configuration settings,
    /// such as the connection string.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> with the registered infrastructure dependencies.
    /// </returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}