namespace MiasoftNanus.PhoneBook.WebApi.WorkAreas.Dtos;

/// <summary>
/// Data transfer object representing a work area.
/// </summary>
public record WorkAreaDto(
    Guid Id,
    string Code,
    string Name,
    string? Description
);
