namespace MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

/// <summary>
/// Defines the contract for saving, reading, and deleting files in a storage.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Asynchronously saves the content in the given container with a generated, unique name.
    /// </summary>
    /// <param name="content">The file content.</param>
    /// <param name="extension">The file extension, including the dot (e.g. <c>.png</c>). Already validated.</param>
    /// <param name="container">The logical folder (e.g. <c>agendas</c>).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The saved file.</returns>
    Task<StoredFile> SaveAsync(
        Stream content,
        string extension,
        string container,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously opens the file at the given relative path for reading.
    /// </summary>
    /// <param name="path">The relative path returned by <see cref="SaveAsync"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A readable stream the caller must dispose, or <c>null</c> if the file does not exist.</returns>
    Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes the file at the given relative path. Does nothing if it does not exist.
    /// </summary>
    /// <param name="path">The relative path returned by <see cref="SaveAsync"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
