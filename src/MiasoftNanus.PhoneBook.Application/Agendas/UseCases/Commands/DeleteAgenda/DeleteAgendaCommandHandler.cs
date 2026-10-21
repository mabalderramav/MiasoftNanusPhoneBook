using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeleteAgenda;

/// <summary>
/// Deletes and persists removal of an existing agenda.
/// </summary>
public sealed class DeleteAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteAgendaCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure(AgendaErrors.AgendaNotFound);

        agendaRepository.Remove(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success() : Result.Failure(Error.CannotSaveChanges);
    }
}
