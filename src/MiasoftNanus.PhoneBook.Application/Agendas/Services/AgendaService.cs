using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Services;
using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.CreateAgenda;
using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeleteAgenda;
using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.PatchAgenda;
using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UpdateAgenda;
using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetAgendaAll;
using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetAgendaById;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Services;

namespace MiasoftNanus.PhoneBook.Application.Agendas.Services;

/// <summary>
/// Provides CRUD functionality for agendas through the CQRS dispatcher.
/// </summary>
public class AgendaService(IDispatcher dispatcher)
    : CrudCqrsService<Agenda, Guid>(dispatcher), IAgendaService
{
    protected override ICommand<Result<Agenda>> BuildCreateCommand(Agenda entity)
    {
        return new CreateAgendaCommand(
            entity.Code,
            entity.FullName,
            entity.Email!.Value,
            entity.Enabled,
            entity.ImagePath,
            entity.DirectLine,
            entity.Internal,
            entity.Corporate,
            entity.DirectAccess,
            entity.WorkstationId,
            entity.LocationId,
            entity.UserIdCreated);
    }

    protected override IQuery<Result<Agenda>> BuildGetByIdQuery(Guid id)
    {
        return new GetAgendaByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<Agenda>>> BuildGetAllQuery()
    {
        return new GetAgendaAllQuery();
    }

    protected override ICommand<Result<Agenda>> BuildUpdateCommand(Guid id, Agenda entity)
    {
        return new UpdateAgendaCommand(
            id,
            entity.Code,
            entity.FullName,
            entity.Email!.Value,
            entity.Enabled,
            entity.ImagePath,
            entity.DirectLine,
            entity.Internal,
            entity.Corporate,
            entity.DirectAccess,
            entity.WorkstationId,
            entity.LocationId,
            entity.UserIdCreated);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteAgendaCommand(id);
    }

    public Task<Result<Agenda>> PatchAsync(Guid id, AgendaPatchData data, CancellationToken cancellationToken = default)
    {
        return Dispatcher.SendCommandAsync(new PatchAgendaCommand(id, data), cancellationToken);
    }
}
