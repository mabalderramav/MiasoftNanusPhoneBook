using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Queries.GetUserById;

/// <summary>
/// Represents a query to retrieve a user by identifier.
/// </summary>
public record GetUserByIdQuery(Guid Id) : IQuery<Result<User>>;
