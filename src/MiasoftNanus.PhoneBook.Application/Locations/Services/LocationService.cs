using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Services;
using MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.CreateLocation;
using MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.DeleteLocation;
using MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.UpdateLocation;
using MiasoftNanus.PhoneBook.Application.Locations.UseCases.Queries.GetLocationAll;
using MiasoftNanus.PhoneBook.Application.Locations.UseCases.Queries.GetLocationById;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Services;

namespace MiasoftNanus.PhoneBook.Application.Locations.Services;

/// <summary>
/// Provides functionality for managing locations within the application.
/// </summary>
/// <remarks>
/// This service extends the base functionality provided by <see cref="CrudCqrsService{T, TId}"/>
/// and implements operations specific to location entities. It interacts with the command
/// and query dispatcher to perform CRUD operations for locations.
/// </remarks>
/// <inheritdoc cref="CrudCqrsService{T, TId}" />
public class LocationService(IDispatcher dispatcher) : CrudCqrsService<Location, Guid>(dispatcher), ILocationService
{
    protected override ICommand<Result<Location>> BuildCreateCommand(Location entity)
    {
        return new CreateLocationCommand(entity.Code, entity.Description);
    }

    protected override IQuery<Result<Location>> BuildGetByIdQuery(Guid id)
    {
        return new GetLocationByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<Location>>> BuildGetAllQuery()
    {
        return new GetLocationAllQuery();
    }

    protected override ICommand<Result<Location>> BuildUpdateCommand(Guid id, Location entity)
    {
        return new UpdateLocationCommand(id, entity.Code, entity.Description);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteLocationCommand(id);
    }
}
