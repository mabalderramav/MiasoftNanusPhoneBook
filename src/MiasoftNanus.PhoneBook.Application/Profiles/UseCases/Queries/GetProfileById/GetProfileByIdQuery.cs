using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Queries.GetProfileById;

/// <summary>
/// Represents a query to retrieve the profile of a user by their unique identifier.
/// This query follows the CQRS architecture, enabling separation of concerns
/// between application reads and writes.
/// </summary>
/// <param name="Id">
/// The unique identifier of the profile to be retrieved.
/// </param>
/// <remarks>
/// The query returns a result encapsulated in the <see cref="Result{Profile}"/> type,
/// which includes the profile data if the operation is successful or the corresponding error information otherwise.
/// </remarks>
public record GetProfileByIdQuery(Guid Id) : IQuery<Result<Profile>>;