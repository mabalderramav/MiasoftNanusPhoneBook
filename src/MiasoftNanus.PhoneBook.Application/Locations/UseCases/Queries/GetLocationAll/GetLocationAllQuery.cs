using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Queries.GetLocationAll;

/// <summary>
/// Represents a query to retrieve all the locations.
/// This query follows the CQRS architecture, enabling separation of concerns
/// between application reads and writes.
/// </summary>
/// <remarks>
/// The query returns a result encapsulated in the <see cref="Result{T}"/> type,
/// which includes the collection of locations if the operation is successful or the corresponding error information otherwise.
/// </remarks>
public record GetLocationAllQuery : IQuery<Result<ICollection<Location>>>;
