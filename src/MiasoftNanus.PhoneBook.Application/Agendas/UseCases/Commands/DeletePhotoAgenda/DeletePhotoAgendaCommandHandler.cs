using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;
using MiasoftNanus.PhoneBook.Application.Agendas.Storage;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeletePhotoAgenda;

/// <summary>
/// Removes the photo of an existing agenda and deletes the stored file.
/// </summary>
public sealed class DeletePhotoAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage
) : ICommandHandler<DeletePhotoAgendaCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeletePhotoAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure(AgendaErrors.AgendaNotFound);

        var imagePath = agenda.ImagePath;
        var removeResult = agenda.RemoveImagePath();
        if (removeResult.IsFailure)
            return removeResult;

        agendaRepository.Update(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
            return Result.Failure(Error.CannotSaveChanges);

        // Only after the save: the agenda must never point to a deleted file.
        await fileStorage.DeleteUnreferencedPhotoAsync(imagePath);
        return Result.Success();
    }
}
