using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Shared.Enums;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.UpdateUser;

/// <summary>
/// Represents a command to update a user.
/// </summary>
public record UpdateUserCommand(
    Guid Id,
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
        $"{nameof(UpdateUserCommand)} {{ Id = {Id}, FirstName = {FirstName}, LastName = {LastName}, Password = ***, " + 
        $"Email = {Email}, States = {States}, ProfileId = {ProfileId} }}";
}