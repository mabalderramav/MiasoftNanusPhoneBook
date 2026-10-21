using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Agendas.Mappers;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UpdateAgenda;

/// <summary>
/// Updates and persists an existing agenda.
/// </summary>
public sealed class UpdateAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IWorkstationRepository workstationRepository,
    ILocationRepository locationRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateAgendaCommand, Result<Agenda>>
{
    public async Task<Result<Agenda>> HandleAsync(
        UpdateAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure<Agenda>(AgendaErrors.AgendaNotFound);

        var updateResult = agenda.Update(command.ToAgendaCreationData());
        if (updateResult.IsFailure)
            return Result.Failure<Agenda>(updateResult.Error);

        var workstation = await workstationRepository.GetByIdAsync(command.WorkstationId, cancellationToken);
        if (workstation is null)
            return Result.Failure<Agenda>(WorkstationErrors.WorkstationNotFound);

        var location = await locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure<Agenda>(LocationErrors.LocationNotFound);

        agendaRepository.Update(agenda);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(agenda)
            : Result.Failure<Agenda>(Error.CannotSaveChanges);
    }
}
