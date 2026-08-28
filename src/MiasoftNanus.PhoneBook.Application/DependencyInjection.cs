using Microsoft.Extensions.DependencyInjection;

namespace MiasoftNanus.PhoneBook.Application;

/// <summary>
/// Provides extension methods for configuring application-level dependency injection.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options => 
            options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}