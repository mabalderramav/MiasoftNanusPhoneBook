using MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;
using MiasoftNanus.PhoneBook.Domain.Shared;

namespace MiasoftNanus.PhoneBook.Application.Users.CreateUser;

/// <summary>
/// Represents a command for creating a new user in the system.
/// </summary>
/// <remarks>
/// This command encapsulates all necessary information required to create a user, including personal details,
/// credentials, location data, and role assignment. It adheres to the principles of the CQRS pattern and is used
/// to trigger the creation of a new user entity within the application.
/// </remarks>
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
    string RoleName
) : ICommand<Guid>;