namespace MiasoftNanus.PhoneBook.Infrastructure.Shared.Configurations;

/// <summary>
/// Represents a container for configuration-related constant values used across the application.
/// This class is intended to centralize and provide strongly-typed access to configuration keys
/// to minimize hardcoded strings throughout the codebase.
/// </summary>
public static class ConfigurationConstants
{
    /// <summary>
    /// Represents the configuration key used to retrieve file storage settings
    /// from the application's configuration.
    /// </summary>
    public const string FileStorageConfig = "FileStorage";

    /// <summary>
    /// Represents the configuration key used to retrieve the default database connection string
    /// from the application's configuration.
    /// </summary>
    public const string DefaultConnection = "DefaultConnection";
}