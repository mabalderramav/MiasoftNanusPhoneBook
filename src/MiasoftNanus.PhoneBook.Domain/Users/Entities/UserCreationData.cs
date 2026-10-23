namespace MiasoftNanus.PhoneBook.Domain.Users.Entities;

/// <summary>
/// Represents the data required to create a User instance: the user details plus the raw password.
/// </summary>
/// <remarks>
/// The <see cref="ObjectValues.Password"/> value object is validated and built by the <see cref="User"/> entity
/// itself.
/// </remarks>
public sealed class UserCreationData : UserData
{
    public required string Password { get; init; }
}
