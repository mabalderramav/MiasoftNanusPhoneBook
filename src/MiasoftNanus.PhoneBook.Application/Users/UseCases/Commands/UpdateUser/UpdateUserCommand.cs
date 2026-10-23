using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Shared.Enums;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.UpdateUser;

/// <summary>
/// Represents a command to update the details of a user. The password is changed by
/// <see cref="ChangeUserPassword.ChangeUserPasswordCommand"/>.
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime Birthdate,
    string Email,
    string Country,
    string State,
    string Province,
    string District,
    string Street,
    States States,
    Guid ProfileId
) : ICommand<Result<User>>;
