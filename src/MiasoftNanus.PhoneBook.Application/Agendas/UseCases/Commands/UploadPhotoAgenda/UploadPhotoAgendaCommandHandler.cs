using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;
using MiasoftNanus.PhoneBook.Application.Agendas.Storage;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UploadPhotoAgenda;

/// <summary>
/// Validates and stores the photo of an existing agenda and persists its new image path.
/// </summary>
public sealed class UploadPhotoAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage
) : ICommandHandler<UploadPhotoAgendaCommand, Result<Agenda>>
{
    private const long MaxPhotoSize = 2 * 1024 * 1024; // 2 MB

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    public async Task<Result<Agenda>> HandleAsync(
        UploadPhotoAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate before touching the storage or the database.
        if (command.Length <= 0)
            return Result.Failure<Agenda>(AgendaErrors.PhotoCannotBeEmpty);

        if (command.Length > MaxPhotoSize)
            return Result.Failure<Agenda>(AgendaErrors.PhotoTooLarge);

        // Only the extension of the client's file name is used, never the name itself.
        var extension = Path.GetExtension(command.FileName);
        if (!AllowedExtensions.Contains(extension))
            return Result.Failure<Agenda>(AgendaErrors.PhotoInvalidFormat);

        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure<Agenda>(AgendaErrors.AgendaNotFound);

        var previousImagePath = agenda.ImagePath;
        var storedFile = await fileStorage.SaveAsync(
            command.Content, extension.ToLowerInvariant(), AgendaPhotoStorage.Container, cancellationToken);

        var changeResult = agenda.ChangeImagePath(storedFile.Path);
        if (changeResult.IsFailure)
        {
            await fileStorage.DeleteAsync(storedFile.Path, CancellationToken.None);
            return Result.Failure<Agenda>(changeResult.Error);
        }

        agendaRepository.Update(agenda);
        int result;
        try
        {
            result = await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Any agenda does not reference the new file: remove it.
            await fileStorage.DeleteAsync(storedFile.Path, CancellationToken.None);
            throw;
        }

        if (result == 0)
        {
            await fileStorage.DeleteAsync(storedFile.Path, CancellationToken.None);
            return Result.Failure<Agenda>(Error.CannotSaveChanges);
        }

        // The previous photo is no longer referenced.
        await fileStorage.DeleteUnreferencedPhotoAsync(previousImagePath);

        await agendaRepository.LoadDetailsAsync(agenda, cancellationToken);
        return Result.Success(agenda);
    }
}
