using MiasoftNanus.PhoneBook.Domain.Shared.Enums;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

namespace MiasoftNanus.PhoneBook.Domain.Users.Entities;

/// <summary>
/// Represents the data required to create or update a User instance.
/// </summary>
/// <remarks>
/// This class serves as a container for the raw user-related values. Value objects such as
/// <see cref="ObjectValues.Password"/> and <see cref="Shared.ObjectValues.Email"/> are validated and built
/// by the <see cref="User"/> entity itself.
/// </remarks>
public sealed class UserCreationData
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Password { get; init; }
    public required DateTime Birthdate { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
    public required States States { get; init; }
    public required DateTime DateOfLastChange { get; init; }
    public required Guid ProfileId { get; init; }
}
