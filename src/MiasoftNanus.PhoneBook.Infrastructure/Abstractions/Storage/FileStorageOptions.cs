namespace MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Storage;

/// <summary>
/// Settings of the local file storage.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// The root folder of the storage. A relative path is resolved against the current directory.
    /// </summary>
    public string RootPath { get; init; } = "Storage";
}
