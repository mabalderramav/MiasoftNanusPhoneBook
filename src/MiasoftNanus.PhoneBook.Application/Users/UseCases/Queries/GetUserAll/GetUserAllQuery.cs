using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Queries.GetUserAll;

/// <summary>
/// Represents a query to retrieve all users.
/// </summary>
public record GetUserAllQuery : IQuery<Result<ICollection<User>>>;
