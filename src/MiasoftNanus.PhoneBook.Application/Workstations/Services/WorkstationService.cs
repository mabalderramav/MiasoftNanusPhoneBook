using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Services;
using MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.CreateWorkstation;
using MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.DeleteWorkstation;
using MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.UpdateWorkstation;
using MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Queries.GetWorkstationAll;
using MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Queries.GetWorkstationById;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Services;

namespace MiasoftNanus.PhoneBook.Application.Workstations.Services;

/// <summary>
/// Provides CRUD functionality for workstations through the CQRS dispatcher.
/// </summary>
public class WorkstationService(IDispatcher dispatcher)
    : CrudCqrsService<Workstation, Guid>(dispatcher), IWorkstationService
{
    protected override ICommand<Result<Workstation>> BuildCreateCommand(Workstation entity)
    {
        return new CreateWorkstationCommand(entity.Code, entity.Name, entity.Description, entity.WorkAreaId);
    }

    protected override IQuery<Result<Workstation>> BuildGetByIdQuery(Guid id)
    {
        return new GetWorkstationByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<Workstation>>> BuildGetAllQuery()
    {
        return new GetWorkstationAllQuery();
    }

    protected override ICommand<Result<Workstation>> BuildUpdateCommand(Guid id, Workstation entity)
    {
        return new UpdateWorkstationCommand(id, entity.Code, entity.Name, entity.Description, entity.WorkAreaId);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteWorkstationCommand(id);
    }
}
