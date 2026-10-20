namespace MiasoftNanus.PhoneBook.WebApi.Locations.Dtos;

/// <summary>
/// Data Transfer Object used for creating or updating a location.
/// </summary>
public record CreateUpdateLocationDto(
    string Code,
    string Description
);
