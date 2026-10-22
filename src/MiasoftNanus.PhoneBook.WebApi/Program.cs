using MiasoftNanus.PhoneBook.WebApi.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = builder.Logging.InitLogging();

try
{
    logger.Information(
        "LOG INITIALIZED in {GetEnvironmentVariable}",
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "ENVIRONMENT NOT DEFINED."
    );

    #region Services

    builder.Services.AddOpenApi();
    builder.Services.AddApplicationConfigurationServices(builder.Configuration);
    builder.Services.AddApplicationLayersServices(builder.Configuration);

    #endregion

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (builder.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        await app.MigrateDatabaseAsync();
    }

    // Do not enable request/response body logging (e.g., UseHttpLogging with RequestBody) without excluding
    // /users: it would log plain-text passwords.
    app.UseHttpsRedirection();
    app.MapEndpoints();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    logger.Fatal(ex, "An unhandled exception has occurred in the middleware of the application");
}
finally
{
    await Log.CloseAndFlushAsync();
}