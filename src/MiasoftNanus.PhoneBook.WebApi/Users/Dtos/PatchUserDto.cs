using MiasoftNanus.PhoneBook.Domain.Shared.Enums;

namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object used for partially updating the details of a user (JSON Merge Patch). A missing or
/// <c>null</c> member keeps the current value. The password is changed through <see cref="ChangeUserPasswordDto"/>.
/// </summary>
public record PatchUserDto(
    string? FirstName,
    string? LastName,
    DateTime? Birthdate,
    string? Email,
    PatchAddressDto? Address,
    States? States,
    Guid? ProfileId
);
