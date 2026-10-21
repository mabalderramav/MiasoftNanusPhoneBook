using MiasoftNanus.PhoneBook.Domain.Shared.Enums;

namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object used for creating or updating a user.
/// </summary>
public record CreateUpdateUserDto(
    string FirstName,
    string LastName,
    string Password,
    DateTime Birthdate,
    string Email,
    AddressDto? Address,
    States States,
    Guid ProfileId
);
