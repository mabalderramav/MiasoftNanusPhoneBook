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

    public Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveInsideRoot(path);
        if (fullPath is null)
            return Task.FromResult<Stream?>(null);

        try
        {
            // FileShare.Delete: a concurrent upload may delete this (previous) photo while it is being sent.
            Stream stream = new FileStream(
                fullPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete,
                bufferSize: 4096, useAsync: true);
            return Task.FromResult<Stream?>(stream);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return Task.FromResult<Stream?>(null);
        }
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveInsideRoot(path);
        if (fullPath is not null && File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    // Returns the physical path, or null if it would be outside the storage root
    // (e.g., an ImagePath set to "../../x" through PUT / PATCH).
    private string? ResolveInsideRoot(string path)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, path));
        return fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? fullPath
            : null;
    }
}
