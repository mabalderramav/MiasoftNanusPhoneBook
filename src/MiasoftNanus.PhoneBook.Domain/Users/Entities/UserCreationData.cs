using MiasoftNanus.PhoneBook.Domain.Shared;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

namespace MiasoftNanus.PhoneBook.Domain.Users.Entities;

/// <summary>
/// Represents the necessary data required for the creation of a User instance.
/// </summary>
/// <remarks>
/// This class serves as a container for user-related properties that are essential
/// during the initialization of a new user object in the domain.
/// </remarks>
public sealed class UserCreationData
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required Password Password { get; init; }
    public required DateTime Birthdate { get; init; }
    public required Email Email { get; init; }
    public required Address Address { get; init; }
    public required States States { get; init; }
    public required DateTime DateOfLastChange { get; init; }
    public required Guid ProfileId { get; init; }
}