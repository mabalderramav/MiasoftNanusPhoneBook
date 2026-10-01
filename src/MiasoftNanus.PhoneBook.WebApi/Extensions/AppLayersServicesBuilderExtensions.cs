using MiasoftNanus.PhoneBook.Application;
using MiasoftNanus.PhoneBook.Infrastructure;

namespace MiasoftNanus.PhoneBook.WebApi.Extensions;

/// <summary>
/// Provides extension methods for registering services related to the application layers,
/// including the application and infrastructure layers, into the ASP.NET Core dependency
/// injection container.
/// </summary>
public static class AppLayersServicesBuilderExtensions
{
    /// <summary>
    /// Registers application and infrastructure layer services into the specified
    /// dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the services for the application
    /// and infrastructure layers are added.
    /// </param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance that provides access to the application's
    /// configuration settings, used to configure the infrastructure layer.
    /// </param>
    public static void AddApplicationLayersServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
    }
}