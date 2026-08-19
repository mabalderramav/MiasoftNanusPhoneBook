using MiasoftNanus.PhoneBook.WebApi.Config;
using Microsoft.Extensions.Options;

namespace MiasoftNanus.PhoneBook.WebApi.Endpoints;

/// <summary>
/// Provides a collection of endpoints related to health check functionalities within the API.
/// </summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
        app.MapGet("/version", () => Results.Ok(new { version = "1.0.0" }));
        app.MapGet("/info", () => Results.Ok(new
        {
            name = "Miasoft Nanus PhoneBook API",
            version = "1.0.0",
            description = "A simple phone book API built with ASP.NET Core."
        }));
        app.MapGet("/status", () => Results.Ok(new { status = "Running", timestamp = DateTime.UtcNow }));
        app.MapGet("/metrics", () => Results.Ok(new
        {
            uptime =
                (DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds,
            memoryUsage = GC.GetTotalMemory(false),
            threadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count
        }));
        app.MapGet("/Config", (IConfiguration configuration, IOptions<ApiConfig> apiConfigOptions) =>
        {
            var apiConfig = apiConfigOptions.Value;
            var environment = configuration["ASPNETCORE_ENVIRONMENT"];
            var connectionString = configuration.GetConnectionString("ConnectionSql");
            var timeout = apiConfig.Timeout;
            var baseUrl = apiConfig.BaseUrl;
            var token = apiConfig.Token;
            return Results.Ok(new
            {
                environment,
                connectionString,
                timeout,
                baseUrl,
                token
            });
        });
    }
}