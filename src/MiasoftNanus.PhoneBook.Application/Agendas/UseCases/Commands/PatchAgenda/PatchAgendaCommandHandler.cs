using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.PatchAgenda;

/// <summary>
/// Applies and persists a partial update of an existing agenda.
/// </summary>
public sealed class PatchAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IWorkstationRepository workstationRepository,
    ILocationRepository locationRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<PatchAgendaCommand, Result<Agenda>>
{
    public async Task<Result<Agenda>> HandleAsync(
        PatchAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure<Agenda>(AgendaErrors.AgendaNotFound);

        // Nothing to change: no save (it would affect 0 rows).
        if (!command.Data.HasChanges)
        {
            await agendaRepository.LoadDetailsAsync(agenda, cancellationToken);
            return Result.Success(agenda);
        }

        var patchResult = agenda.Patch(command.Data);
        if (patchResult.IsFailure)
            return Result.Failure<Agenda>(patchResult.Error);

        if (command.Data.WorkstationId is not null)
        {
            var workstation =
                await workstationRepository.GetByIdAsync(command.Data.WorkstationId.Value, cancellationToken);
            if (workstation is null)
                return Result.Failure<Agenda>(WorkstationErrors.WorkstationNotFound);
        }

        if (command.Data.LocationId is not null)
        {
            var location = await locationRepository.GetByIdAsync(command.Data.LocationId.Value, cancellationToken);
            if (location is null)
                return Result.Failure<Agenda>(LocationErrors.LocationNotFound);
        }

        agendaRepository.Update(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
            return Result.Failure<Agenda>(Error.CannotSaveChanges);

        await agendaRepository.LoadDetailsAsync(agenda, cancellationToken);
        return Result.Success(agenda);
    }
}
