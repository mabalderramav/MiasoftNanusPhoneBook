using MiasoftNanus.PhoneBook.Domain.Shared.Enums;

namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object used for updating the details of a user. The password is changed through
/// <see cref="ChangeUserPasswordDto"/>.
/// </summary>
public record UpdateUserDto(
    string FirstName,
    string LastName,
    DateTime Birthdate,
    string Email,
    AddressDto? Address,
    States States,
    Guid ProfileId
);
