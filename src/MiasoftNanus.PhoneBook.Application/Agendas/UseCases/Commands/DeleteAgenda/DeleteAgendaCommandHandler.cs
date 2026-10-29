using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;
using MiasoftNanus.PhoneBook.Application.Agendas.Storage;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeleteAgenda;

/// <summary>
/// Deletes and persists removal of an existing agenda, and deletes its photo.
/// </summary>
public sealed class DeleteAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage
) : ICommandHandler<DeleteAgendaCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure(AgendaErrors.AgendaNotFound);

        var imagePath = agenda.ImagePath;
        agendaRepository.Remove(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
            return Result.Failure(Error.CannotSaveChanges);

        // Only after the save: the photo is no longer referenced.
        await fileStorage.DeleteUnreferencedPhotoAsync(imagePath);
        return Result.Success();
    }
}
