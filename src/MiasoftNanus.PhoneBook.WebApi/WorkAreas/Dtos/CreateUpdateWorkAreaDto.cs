namespace MiasoftNanus.PhoneBook.WebApi.WorkAreas.Dtos;

/// <summary>
/// Data transfer object used for creating or updating a work area.
/// </summary>
public record CreateUpdateWorkAreaDto(
    string Code,
    string Name,
    string? Description
);
