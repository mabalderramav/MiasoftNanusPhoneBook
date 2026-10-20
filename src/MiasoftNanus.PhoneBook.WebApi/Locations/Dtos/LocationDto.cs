namespace MiasoftNanus.PhoneBook.WebApi.Locations.Dtos;

/// <summary>
/// Data transfer object representing a location.
/// </summary>
public record LocationDto(
    Guid Id,
    string Code,
    string Description
);
