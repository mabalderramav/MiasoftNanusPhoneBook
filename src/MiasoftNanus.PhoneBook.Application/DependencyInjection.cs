using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace MiasoftNanus.PhoneBook.Application;

/// <summary>
/// Provides extension methods for configuring application-level dependency injection.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // Register all command and query handlers dynamically.
        var interfacesDeHandler = new[] { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };

        // Use LINQ to find all classes that implement ICommandHandler<, >
        // or IQueryHandler<, > and register them with the DI container.
        var registeredHandlers =
            from typeInfo in typeof(DependencyInjection).Assembly.GetTypes()
            where typeInfo is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
            from implementedInterface in typeInfo.GetInterfaces()
            where implementedInterface.IsGenericType
                  && interfacesDeHandler.Contains(implementedInterface.GetGenericTypeDefinition())
            select (implementedInterface, typeInfo);

        // Register each handler interface and its corresponding implementation type with the DI container.
        foreach (var (handlerInterface, implementationType) in registeredHandlers)
        {
            services.AddScoped(handlerInterface, implementationType);
        }

        return services;
    }
}