using MiasoftNanus.PhoneBook.Domain.Shared.Enums;

namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object representing a user. The password is never exposed.
/// </summary>
public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string UserName,
    DateTime Birthdate,
    string Email,
    AddressDto Address,
    States States,
    DateTime DateOfLastChange,
    Guid ProfileId
);
