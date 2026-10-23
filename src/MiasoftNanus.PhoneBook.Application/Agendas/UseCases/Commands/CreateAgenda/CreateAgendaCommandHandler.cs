using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Agendas.Mappers;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.CreateAgenda;

/// <summary>
/// Creates and persists a new agenda.
/// </summary>
public sealed class CreateAgendaCommandHandler(
    IAgendaRepository agendaRepository,
    IWorkstationRepository workstationRepository,
    ILocationRepository locationRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateAgendaCommand, Result<Agenda>>
{
    public async Task<Result<Agenda>> HandleAsync(
        CreateAgendaCommand command,
        CancellationToken cancellationToken = default)
    {
        var agendaResult = Agenda.Create(command.ToAgendaCreationData());
        if (agendaResult.IsFailure)
            return Result.Failure<Agenda>(agendaResult.Error);

        var workstation = await workstationRepository.GetByIdAsync(command.WorkstationId, cancellationToken);
        if (workstation is null)
            return Result.Failure<Agenda>(WorkstationErrors.WorkstationNotFound);

        var location = await locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure<Agenda>(LocationErrors.LocationNotFound);

        var user = await userRepository.GetByIdAsync(command.UserIdCreated, cancellationToken);
        if (user is null)
            return Result.Failure<Agenda>(UserErrors.UserNotFound);

        agendaRepository.Add(agendaResult.Value);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result == 0)
            return Result.Failure<Agenda>(Error.CannotSaveChanges);

        await agendaRepository.LoadDetailsAsync(agendaResult.Value, cancellationToken);
        return Result.Success(agendaResult.Value);
    }
}
