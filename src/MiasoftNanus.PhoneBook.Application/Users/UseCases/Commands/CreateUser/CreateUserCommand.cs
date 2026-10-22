using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Shared.Enums;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.CreateUser;

/// <summary>
/// Represents a command to create a user.
/// </summary>
public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Password,
    DateTime Birthdate,
    string Email,
    string Country,
    string State,
    string Province,
    string District,
    string Street,
    States States,
    Guid ProfileId
) : ICommand<Result<User>>
{
    // Masks the raw password so it never leaks through logs.
    public override string ToString() =>
        $"{nameof(CreateUserCommand)} {{ FirstName = {FirstName}, LastName = {LastName}, Password = ***, " +
        $"Email = {Email}, States = {States}, ProfileId = {ProfileId} }}";
}