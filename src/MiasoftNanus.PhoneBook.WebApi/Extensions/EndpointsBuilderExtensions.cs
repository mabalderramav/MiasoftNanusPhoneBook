using MiasoftNanus.PhoneBook.WebApi.Endpoints;

namespace MiasoftNanus.PhoneBook.WebApi.Extensions;

/// <summary>
/// Provides extension methods for configuring and mapping application-specific API endpoints
/// to an <see cref="IEndpointRouteBuilder"/> instance.
/// </summary>
public static class EndpointsBuilderExtensions
{
    /// <summary>
    /// Maps the API endpoints specific to the Miasoft Nanus PhoneBook application.
    /// </summary>
    /// <param name="endpoints">
    /// The <see cref="IEndpointRouteBuilder"/> instance used to define and configure routes.
    /// </param>
    public static void MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => "Hello World! - Oscar Martin Balderrama Vaca - Miasoft Nanus PhoneBook API");
        endpoints.MapHealthEndpoints();
    }
}