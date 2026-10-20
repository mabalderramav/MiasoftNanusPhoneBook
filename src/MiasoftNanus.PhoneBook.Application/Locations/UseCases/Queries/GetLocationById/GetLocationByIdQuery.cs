using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Queries.GetLocationById;

/// <summary>
/// Represents a query to retrieve a location by its unique identifier.
/// This query follows the CQRS architecture, enabling separation of concerns
/// between application reads and writes.
/// </summary>
/// <param name="Id">
/// The unique identifier of the location to be retrieved.
/// </param>
/// <remarks>
/// The query returns a result encapsulated in the <see cref="Result{Location}"/> type,
/// which includes the location data if the operation is successful or the corresponding error information otherwise.
/// </remarks>
public record GetLocationByIdQuery(Guid Id) : IQuery<Result<Location>>;
