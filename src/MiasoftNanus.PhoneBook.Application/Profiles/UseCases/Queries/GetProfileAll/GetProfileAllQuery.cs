using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Queries.GetProfileAll;

/// <summary>
/// Represents a query to retrieve all the profiles.
/// This query follows the CQRS architecture, enabling separation of concerns
/// between application reads and writes.
/// </summary>
/// <remarks>
/// The query returns a result encapsulated in the <see cref="Result{T}"/> type,
/// which includes the collection of profiles if the operation is successful or the corresponding error information otherwise.
/// </remarks>
public record GetProfileAllQuery : IQuery<Result<ICollection<Profile>>>;
