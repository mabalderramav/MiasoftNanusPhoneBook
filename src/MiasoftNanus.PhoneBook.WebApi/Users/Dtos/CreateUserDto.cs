using MiasoftNanus.PhoneBook.Domain.Shared.Enums;

namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object used for creating a user.
/// </summary>
public record CreateUserDto(
    string FirstName,
    string LastName,
    string Password,
    DateTime Birthdate,
    string Email,
    AddressDto? Address,
    States States,
    Guid ProfileId
)
{
    // Masks the raw password so it never leaks through logs.
    public override string ToString() =>
        $"{nameof(CreateUserDto)} {{ FirstName = {FirstName}, LastName = {LastName}, Password = ***, " +
        $"Email = {Email}, States = {States}, ProfileId = {ProfileId} }}";
}
