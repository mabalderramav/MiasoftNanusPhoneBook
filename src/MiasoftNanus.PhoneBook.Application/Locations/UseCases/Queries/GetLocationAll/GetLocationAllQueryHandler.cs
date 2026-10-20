using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Queries.GetLocationAll;

/// <summary>
/// Handles the query to retrieve all the locations.
/// Implements the <c>IQueryHandler</c> interface to process the <c>GetLocationAllQuery</c>
/// and return the result wrapped as <c>Result&lt;ICollection&lt;Location&gt;&gt;</c>.
/// </summary>
/// <remarks>
/// This handler depends on the <c>ILocationRepository</c> to fetch the locations from the data source.
/// It returns a success result if locations are found; otherwise, it returns a failure result.
/// </remarks>
public class GetLocationAllQueryHandler(ILocationRepository locationRepository) :
    IQueryHandler<GetLocationAllQuery, Result<ICollection<Location>>>
{
    public async Task<Result<ICollection<Location>>> HandleAsync(GetLocationAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var locations = await locationRepository.GetAllAsync(cancellationToken);
        return locations.Count > 0
            ? Result.Success(locations)
            : Result.Failure<ICollection<Location>>(LocationErrors.LocationsNotFound);
    }
}
