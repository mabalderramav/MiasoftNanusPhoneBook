using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

namespace MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Storage;

/// <summary>
/// Implements <see cref="IFileStorage"/> on the local file system.
/// </summary>
internal sealed class LocalFileStorage(FileStorageOptions options) : IFileStorage
{
    private readonly string _rootPath = Path.GetFullPath(options.RootPath);

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string extension,
        string container,
        CancellationToken cancellationToken = default)
    {
        // Path.Combine builds correct physical paths on any OS.
        var targetFolder = Path.Combine(_rootPath, container);
        Directory.CreateDirectory(targetFolder);

        // A generated name: the client's name could overwrite files or escape the folder.
        var fileName = $"{Guid.CreateVersion7():N}{extension}";
        var fullPath = Path.Combine(targetFolder, fileName);

        // CreateNew fails instead of overwriting if a name ever collided.
        var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        try
        {
            await using (stream)
            {
                await content.CopyToAsync(stream, cancellationToken);
            }
        }
        catch
        {
            // Do not leave a partial file behind (e.g., canceled or client disconnected).
            File.Delete(fullPath);
            throw;
        }

        // The stored path uses "/" because it is a relative path for clients, not a physical path.
        return new StoredFile(fileName, $"{container}/{fileName}", new FileInfo(fullPath).Length);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, path));

        // Never delete outside the storage root (e.g., an ImagePath set to "../../x" through PUT / PATCH).
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            return Task.CompletedTask;

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
