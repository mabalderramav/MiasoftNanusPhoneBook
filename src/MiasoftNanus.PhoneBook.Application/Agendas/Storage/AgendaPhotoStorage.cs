using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;

namespace MiasoftNanus.PhoneBook.Application.Agendas.Storage;

/// <summary>
/// Groups the storage rules of agenda photos.
/// </summary>
internal static class AgendaPhotoStorage
{
    /// <summary>
    /// The logical folder where agenda photos are stored.
    /// </summary>
    public const string Container = "agendas";

    /// <summary>
    /// Deletes a photo that is no longer referenced. Paths that do not come from this storage (e.g., set through
    /// POST / PUT / PATCH) are ignored.
    /// </summary>
    /// <param name="fileStorage">The storage of the photo.</param>
    /// <param name="imagePath">The relative path of the photo, or <c>null</c> if there is none.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DeleteUnreferencedPhotoAsync(this IFileStorage fileStorage, string? imagePath)
    {
        if (imagePath is null || !imagePath.StartsWith($"{Container}/", StringComparison.Ordinal))
            return;

        try
        {
            await fileStorage.DeleteAsync(imagePath, CancellationToken.None);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The database is already committed: a failed delete only leaves an orphan file.
        }
    }
}
