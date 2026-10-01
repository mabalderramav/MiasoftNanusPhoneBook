using MiasoftNanus.PhoneBook.WebApi.Config;

namespace MiasoftNanus.PhoneBook.WebApi.Extensions;

/// <summary>
/// A static class providing extension methods for configuring application services in the DI container.
/// </summary>
public static class AppConfigServicesBuilderExtensions
{
    /// <summary>
    /// Adds application configuration services to the dependency injection container,
    /// including the configuration of the ApiConfig settings.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the services are added.
    /// </param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> containing the application settings.
    /// </param>
    public static void AddApplicationConfigurationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiConfig>(configuration.GetSection("API"));
    }
}