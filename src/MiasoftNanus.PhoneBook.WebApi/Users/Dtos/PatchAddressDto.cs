namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object representing a partial user address. A missing member keeps the current value.
/// </summary>
public record PatchAddressDto(
    string? Country,
    string? State,
    string? Province,
    string? District,
    string? Street
);
