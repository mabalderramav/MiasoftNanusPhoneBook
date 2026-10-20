using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Queries.GetLocationById;

/// <summary>
/// Handles the query to retrieve a location by its unique identifier.
/// Implements the <c>IQueryHandler</c> interface to process the <c>GetLocationByIdQuery</c>
/// and return the result wrapped as <c>Result&lt;Location&gt;</c>.
/// </summary>
/// <remarks>
/// This handler depends on the <c>ILocationRepository</c> to fetch the location data from the data source.
/// If a location with the specified identifier is found, it returns a success result containing the location instance.
/// Otherwise, a failure result is returned, indicating that the location was not found.
/// </remarks>
public class GetLocationByIdQueryHandler(ILocationRepository locationRepository) :
    IQueryHandler<GetLocationByIdQuery, Result<Location>>
{
    public async Task<Result<Location>> HandleAsync(GetLocationByIdQuery query, CancellationToken cancellationToken = default)
    {
        var location = await locationRepository.GetByIdAsync(query.Id, cancellationToken);
        return location is not null ? Result.Success(location) : Result.Failure<Location>(LocationErrors.LocationNotFound);
    }
}
