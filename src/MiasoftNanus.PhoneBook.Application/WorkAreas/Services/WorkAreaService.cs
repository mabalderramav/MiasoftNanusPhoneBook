using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Services;
using MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.CreateWorkArea;
using MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.DeleteWorkArea;
using MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.UpdateWorkArea;
using MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Queries.GetWorkAreaAll;
using MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Queries.GetWorkAreaById;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Services;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.Services;

/// <summary>
/// Provides CRUD functionality for work areas through the CQRS dispatcher.
/// </summary>
public class WorkAreaService(IDispatcher dispatcher) : CrudCqrsService<WorkArea, Guid>(dispatcher), IWorkAreaService
{
    protected override ICommand<Result<WorkArea>> BuildCreateCommand(WorkArea entity)
    {
        return new CreateWorkAreaCommand(entity.Code, entity.Name, entity.Description);
    }

    protected override IQuery<Result<WorkArea>> BuildGetByIdQuery(Guid id)
    {
        return new GetWorkAreaByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<WorkArea>>> BuildGetAllQuery()
    {
        return new GetWorkAreaAllQuery();
    }

    protected override ICommand<Result<WorkArea>> BuildUpdateCommand(Guid id, WorkArea entity)
    {
        return new UpdateWorkAreaCommand(id, entity.Code, entity.Name, entity.Description);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteWorkAreaCommand(id);
    }
}
