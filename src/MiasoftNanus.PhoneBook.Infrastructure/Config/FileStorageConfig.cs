namespace MiasoftNanus.PhoneBook.Infrastructure.Config;

/// <summary>
/// Represents the configuration settings for file storage in the application.
/// </summary>
public class FileStorageConfig
{
    /// <summary>
    /// Gets or sets the root directory path for file storage.
    /// </summary>
    public string RootPath { get; set; } = "Storage";
}